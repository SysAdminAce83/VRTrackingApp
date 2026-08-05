using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class ControlEvidence
{
    public int Id { get; set; }
    public int ComplianceControlId { get; set; }
    public ComplianceControl Control { get; set; } = default!;
    public int? FindingComplianceLinkId { get; set; }
    public FindingComplianceLink? FindingLink { get; set; }
    public string Description { get; set; } = default!;
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileHash { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class EvidenceAttachment
{
    public int Id { get; set; }
    public int ControlEvidenceId { get; set; }
    public ControlEvidence Evidence { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string StoredFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; } = default!;
    public string UploadedBy { get; set; } = default!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}