using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Notifications;

namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>
/// P4 — hosted exception lifecycle engine. Sweeps active / review-due exceptions
/// on a timer and performs: expiry (Active / ReviewDue → Expired), the review-due
/// transition (Active → ReviewDue), pre-expiry reminders (30 / 15 / 7 days) and
/// compliance flags (missing evidence, overdue mitigation). Notifications are
/// dispatched best-effort via <see cref="INotificationService"/>.
/// </summary>
public class ExceptionLifecycleService
{
    private static readonly int[] ReminderDays = { 30, 15, 7 };

    // In-process dedupe so repeated sweeps don't re-notify for the same event
    // within a process lifetime (the hosted sweep runs every few minutes).
    private static readonly HashSet<string> Notified = new();

    private readonly VRTrackingAppContext _db;
    private readonly INotificationService _notify;
    private readonly ILogger<ExceptionLifecycleService>? _log;

    public ExceptionLifecycleService(
        VRTrackingAppContext db,
        INotificationService notify,
        ILogger<ExceptionLifecycleService>? log = null)
    {
        _db = db;
        _notify = notify;
        _log = log;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var exceptions = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance)
            .Include(e => e.Owner)
            .Include(e => e.Evidence)
            .Include(e => e.Mitigations)
            .Include(e => e.ApprovalSteps)
            .Where(e => e.Status == ExceptionStatus.ActiveException || e.Status == ExceptionStatus.ReviewDue)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var changes = false;

        foreach (var ex in exceptions)
        {
            ct.ThrowIfCancellationRequested();

            // 1) Expiry — supersedes every other transition.
            if (ex.ExpiryDate.HasValue && ex.ExpiryDate.Value <= now)
            {
                ex.Status = ExceptionStatus.Expired;
                if (ex.VulnerabilityInstance?.Status == "Exception") ex.VulnerabilityInstance.Status = "Open";
                await NotifyOnceAsync("expired", ex.Id,
                    (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.ExceptionExpired, roles, id, title, msg, token),
                    new[] { AppRoles.Ciso, AppRoles.RiskCommittee }, ex.Id,
                    "Exception expired",
                    $"Exception #{ex.Id} expired on {ex.ExpiryDate:yyyy-MM-dd}. Remediate or renew it.",
                    ct);
                changes = true;
                continue;
            }

            // 1b) Auto-close: the underlying vulnerability was remediated on a later
            //     scan (instance no longer Open/Exception) — close the exception as Patched.
            if (ex.Status is ExceptionStatus.ActiveException or ExceptionStatus.ReviewDue
                && ex.VulnerabilityInstance != null
                && ex.VulnerabilityInstance.Status is "Fixed" or "Closed" or "Remediated")
            {
                ex.Status = ExceptionStatus.Closed;
                ex.ClosedReason = ClosedReason.Patched;
                ex.ClosedAt = now;
                ex.CurrentApprovalStage = null;
                await NotifyOnceAsync("autoclosed", ex.Id,
                    (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.ExceptionApproved, roles, id, title, msg, token),
                    new[] { AppRoles.Ciso, AppRoles.RiskCommittee }, ex.Id,
                    "Exception auto-closed",
                    $"Exception #{ex.Id} was auto-closed: the vulnerability is no longer detected (remediated).", ct);
                changes = true;
                continue;
            }

            // 2) Review-due transition.
            if (ex.Status == ExceptionStatus.ActiveException && ex.NextReviewDate.HasValue && ex.NextReviewDate.Value <= now)
            {
                ex.Status = ExceptionStatus.ReviewDue;
                await NotifyOnceAsync("reviewdue", ex.Id,
                    (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.ReviewDue, roles, id, title, msg, token),
                    new[] { AppRoles.Ciso, AppRoles.RiskCommittee, AppRoles.InfrastructureManager, AppRoles.NetworkManager }, ex.Id,
                    "Exception review due",
                    $"Exception #{ex.Id} is due for its periodic review.", ct);
                changes = true;
            }

            // 3) Pre-expiry reminders.
            if (ex.ExpiryDate.HasValue)
            {
                var daysLeft = (int)Math.Ceiling((ex.ExpiryDate.Value - now).TotalDays);
                if (ReminderDays.Contains(daysLeft))
                {
                    await NotifyOnceAsync("reminder", ex.Id,
                        (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.ExceptionExpiring, roles, id, title, msg, token),
                        new[] { AppRoles.Ciso, AppRoles.RiskCommittee }, ex.Id,
                        $"Exception expiring in {daysLeft} days",
                        $"Exception #{ex.Id} expires in {daysLeft} days ({ex.ExpiryDate:yyyy-MM-dd}).", ct);
                    changes = true;
                }
            }

            // 4) Missing-evidence flag.
            if (ex.Evidence.Count == 0)
            {
                await NotifyOnceAsync("evidence", ex.Id,
                    (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.EvidenceMissing, roles, id, title, msg, token),
                    new[] { AppRoles.Ciso, AppRoles.RiskCommittee }, ex.Id,
                    "Exception missing evidence",
                    $"Exception #{ex.Id} is active but has no supporting evidence uploaded.", ct);
                changes = true;
            }

            // 5) Overdue-mitigation flag.
            if (ex.Mitigations.Any(m => m.Status == MitigationStatus.Pending))
            {
                await NotifyOnceAsync("mitigation", ex.Id,
                    (roles, id, title, msg, token) => _notify.NotifyRoleAsync(NotificationType.MitigationOverdue, roles, id, title, msg, token),
                    new[] { AppRoles.Ciso, AppRoles.RiskCommittee }, ex.Id,
                    "Mitigation overdue",
                    $"Exception #{ex.Id} has a mitigation still in 'Pending' status.", ct);
                changes = true;
            }
        }

        if (changes)
            await _db.SaveChangesAsync(ct);
    }

    private async Task NotifyOnceAsync(string kind, int exceptionId,
        Func<IEnumerable<string>, int?, string, string, CancellationToken, Task> notify,
        IEnumerable<string> roles, int? id, string title, string message, CancellationToken ct)
    {
        var key = $"{kind}:{exceptionId}";
        if (!Notified.Add(key)) return;
        try
        {
            await notify(roles, id, title, message, ct);
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Failed to send lifecycle notification '{Key}'.", key);
            Notified.Remove(key); // allow a retry on the next sweep
        }
    }
}
