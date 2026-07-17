using System;

namespace VRTrackingApp.Data.Models;

/// <summary>A discussion comment on an exception.</summary>
public class ExceptionComment
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public int? UserId { get; set; }
    public UserAccount? User { get; set; }
    public string? AuthorDisplayName { get; set; }
    public string Body { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
