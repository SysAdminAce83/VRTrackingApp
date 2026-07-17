using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>
/// Creates in-app <see cref="Notification"/> rows and dispatches email via any
/// registered <see cref="INotificationChannel"/>. Email is best-effort: if no channel
/// is configured, only the in-app record is created.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly VRTrackingAppContext _db;
    private readonly UserNotificationService _users;
    private readonly INotificationChannel? _email;
    private readonly EmailTemplateService _templates;
    private readonly IHttpContextAccessor? _http;

    public NotificationService(
        VRTrackingAppContext db,
        UserNotificationService users,
        EmailTemplateService templates,
        INotificationChannel? email = null,
        IHttpContextAccessor? http = null)
    {
        _db = db;
        _users = users;
        _templates = templates;
        _email = email;
        _http = http;
    }

    /// <summary>Notify every active user holding one of the given roles (in-app + email).</summary>
    public async Task NotifyRoleAsync(NotificationType type, IEnumerable<string> roles, int? exceptionId,
        string title, string message, CancellationToken ct = default)
    {
        var recipients = await _users.UsersInRolesAsync(roles);
        await PersistAndSendAsync(recipients, type, exceptionId, title, message, ct);
    }

    /// <summary>Notify a single user (in-app + email).</summary>
    public async Task NotifyUserAsync(UserAccount user, NotificationType type, int? exceptionId,
        string title, string message, CancellationToken ct = default)
    {
        await PersistAndSendAsync(new[] { user }, type, exceptionId, title, message, ct);
    }

    private async Task PersistAndSendAsync(IEnumerable<UserAccount> recipients, NotificationType type,
        int? exceptionId, string title, string message, CancellationToken ct)
    {
        var link = ExceptionLink(exceptionId);
        foreach (var u in recipients)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = u.Id,
                Type = type,
                ExceptionRecordId = exceptionId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = System.DateTime.UtcNow
            });

            if (_email != null && !string.IsNullOrWhiteSpace(u.Email))
            {
                var reference = exceptionId.HasValue ? $"Exception #{exceptionId}" : null;
                var html = _templates.Render(type, title, message, reference, link, u.DisplayName);
                await _email.SendAsync(u.Email, title, html, ct);
            }
        }
        await _db.SaveChangesAsync(ct);
    }

    private string ExceptionLink(int? exceptionId)
    {
        if (exceptionId == null || _http?.HttpContext?.Request == null) return "(exception)";
        var req = _http.HttpContext.Request;
        return $"{req.Scheme}://{req.Host}/Exceptions/Details/{exceptionId}";
    }
}
