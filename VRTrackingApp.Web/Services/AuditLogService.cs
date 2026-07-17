using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services;

/// <summary>
/// Records an append-only audit trail of significant user actions
/// (user management, exception workflow, etc.). Answers "who / when / what".
/// </summary>
public class AuditLogService
{
    private readonly VRTrackingAppContext _db;

    public AuditLogService(VRTrackingAppContext db) => _db = db;

    public async Task LogAsync(string category, string action, string? target, string? detail, int? performedByUserId)
    {
        string? displayName = null;
        if (performedByUserId is > 0)
        {
            var u = await _db.UserAccounts.FindAsync(performedByUserId.Value);
            displayName = u?.DisplayName ?? u?.UserName;
        }

        _db.AuditLogs.Add(new AuditLog
        {
            PerformedByUserId = performedByUserId,
            PerformedByDisplayName = displayName ?? "System",
            Category = category,
            Action = action,
            Target = target,
            Detail = detail,
            PerformedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public static int? CurrentUserId(ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) && uid > 0 ? uid : null;
}
