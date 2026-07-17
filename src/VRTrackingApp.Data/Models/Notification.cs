using System;

namespace VRTrackingApp.Data.Models;

/// <summary>A per-user notification (in-app; optionally emailed).</summary>
public class Notification
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public UserAccount? User { get; set; }

    public NotificationType Type { get; set; }
    public int? ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public string Title { get; set; } = default!;
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EmailedAt { get; set; }
}
