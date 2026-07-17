namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>A delivery channel for notifications (email, Teams, …).</summary>
public interface INotificationChannel
{
    /// <summary>Best-effort send. Returns false if the channel is not configured/available.</summary>
    Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}
