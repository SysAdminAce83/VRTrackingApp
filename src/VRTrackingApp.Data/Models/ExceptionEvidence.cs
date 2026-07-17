using System;

namespace VRTrackingApp.Data.Models;

/// <summary>Section 11 — an uploaded evidence file supporting the exception.</summary>
public class ExceptionEvidence
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public EvidenceType EvidenceType { get; set; } = EvidenceType.Other;
    public string OriginalFileName { get; set; } = default!;
    public string StoredFileName { get; set; } = default!;
    public string? ContentHash { get; set; }
    public long SizeBytes { get; set; }
    public int? UploadedByUserId { get; set; }
    public UserAccount? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
