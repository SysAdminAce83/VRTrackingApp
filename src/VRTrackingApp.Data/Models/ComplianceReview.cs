using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class ComplianceReview
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
    public string? ReviewerNotes { get; set; }
    public bool IsException { get; set; }
    public DateTime? ExceptionExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class RiskAcceptance
{
    public int Id { get; set; }
    public int VulnerabilityFindingId { get; set; }
    public VulnerabilityFinding Finding { get; set; } = default!;
    public int ComplianceControlId { get; set; }
    public ComplianceControl Control { get; set; } = default!;
    public string Justification { get; set; } = default!;
    public string AcceptedBy { get; set; } = default!;
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public RiskAcceptanceStatus Status { get; set; } = RiskAcceptanceStatus.Active;
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public enum RiskAcceptanceStatus
{
    Active,
    Expired,
    Revoked,
    Resolved
}