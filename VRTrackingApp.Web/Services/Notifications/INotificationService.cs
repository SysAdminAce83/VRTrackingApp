using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>In-app + email notification dispatch for the exception lifecycle.</summary>
public interface INotificationService
{
    Task NotifyRoleAsync(NotificationType type, IEnumerable<string> roles, int? exceptionId, string title, string message, CancellationToken ct = default);
    Task NotifyUserAsync(UserAccount user, NotificationType type, int? exceptionId, string title, string message, CancellationToken ct = default);
}
