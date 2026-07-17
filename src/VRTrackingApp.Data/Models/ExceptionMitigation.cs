using System;

namespace VRTrackingApp.Data.Models;

/// <summary>Section 10 — a compensating mitigation applied for the exception.</summary>
public class ExceptionMitigation
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public string Description { get; set; } = default!;
    public MitigationStatus Status { get; set; } = MitigationStatus.Planned;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
