using System;

namespace VRTrackingApp.Data.Models;

/// <summary>Section 13/14 — a periodic review of an active exception.</summary>
public class ExceptionReviewHistory
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public DateTime DueDate { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }
    public UserAccount? ReviewedBy { get; set; }
    public ReviewOutcome Outcome { get; set; } = ReviewOutcome.Pending;
    public string? Comment { get; set; }
}
