using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class FindingComplianceLink
{
    public int Id { get; set; }
    public int VulnerabilityFindingId { get; set; }
    public VulnerabilityFinding Finding { get; set; } = default!;
    public int ComplianceControlId { get; set; }
    public ComplianceControl Control { get; set; } = default!;
    public ComplianceStatus Status { get; set; }
    public string? Rationale { get; set; }
    public string? EvidenceRef { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum ComplianceStatus
{
    NotMapped,
    InProgress,
    Compliant,
    NonCompliant,
    NotApplicable,
    UnderReview
}