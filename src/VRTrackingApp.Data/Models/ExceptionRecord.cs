using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class ExceptionRecord
{
    public int Id { get; set; }
    public int VulnerabilityInstanceId { get; set; }
    public VulnerabilityInstance? VulnerabilityInstance { get; set; }

    public string Reason { get; set; } = default!;
    public int? ApprovedByUserId { get; set; }
    public UserAccount? ApprovedBy { get; set; }

    /// <summary>Legacy expiry (V1). Superseded by <see cref="ExpiryDate"/>; kept for back-compat.</summary>
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Ownership / requester (V1 workflow retained) ---
    /// <summary>User who raised/owns this exception. Null = owned by security team.</summary>
    public int? OwnerUserId { get; set; }
    public UserAccount? Owner { get; set; }

    // --- Legacy V1 modify/delete approval fields (retained; not used by V2 flow) ---
    public string State { get; set; } = ExceptionStates.Active;
    public string? PendingAction { get; set; }
    public string? PendingReason { get; set; }
    public DateTime? PendingExpiresAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? ActionedByUserId { get; set; }
    public UserAccount? ActionedBy { get; set; }

    // =========================================================================
    // V2 — enterprise exception workflow
    // =========================================================================

    // Lifecycle
    public ExceptionStatus Status { get; set; } = ExceptionStatus.Detected;
    /// <summary>Resolved stage-1 approver role: "InfrastructureManager" or "NetworkManager".</summary>
    public string? Stage1Role { get; set; }
    public ApprovalStage? CurrentApprovalStage { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ClosedReason? ClosedReason { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Section 2 — why not fixable
    public NonFixableReason? NonFixableReason { get; set; }
    public string? OtherReasonText { get; set; }

    // Section 3 — technical justification
    public string? TechnicalJustification { get; set; }

    // Section 4 — business justification
    public string? DowntimeConstraint { get; set; }
    public string? BusinessImpact { get; set; }
    public string? CostImpact { get; set; }
    public string? ProductionImpact { get; set; }
    public string? CustomerImpact { get; set; }
    public string? ComplianceImpact { get; set; }

    // Section 5 — risk assessment
    public Likelihood? Likelihood { get; set; }
    public ImpactLevel? Impact { get; set; }
    public RiskLevel? OverallRisk { get; set; }

    // Section 6 — CIA impact
    public bool AffectsConfidentiality { get; set; }
    public bool AffectsIntegrity { get; set; }
    public bool AffectsAvailability { get; set; }

    // Section 7 / 8
    public Exploitability? Exploitability { get; set; }
    public InternetExposure? InternetExposure { get; set; }

    // Section 13 — validity
    public DateTime? StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? ReviewFrequencyDays { get; set; }
    public DateTime? NextReviewDate { get; set; }

    // Children
    public ICollection<ExceptionMitigation> Mitigations { get; set; } = new List<ExceptionMitigation>();
    public ICollection<ExceptionEvidence> Evidence { get; set; } = new List<ExceptionEvidence>();
    public ICollection<ExceptionSecurityControl> SecurityControls { get; set; } = new List<ExceptionSecurityControl>();
    public ICollection<ExceptionApprovalStep> ApprovalSteps { get; set; } = new List<ExceptionApprovalStep>();
    public ICollection<ExceptionReviewHistory> Reviews { get; set; } = new List<ExceptionReviewHistory>();
    public ICollection<ExceptionComment> Comments { get; set; } = new List<ExceptionComment>();
    public ICollection<VendorResponse> VendorResponses { get; set; } = new List<VendorResponse>();
}

// Legacy V1 constants (retained so existing code/migrations keep compiling).
public static class ExceptionStates
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string PendingModification = "PendingModification";
    public const string PendingDeletion = "PendingDeletion";
}

public static class ExceptionPendingActions
{
    public const string Modify = "Modify";
    public const string Delete = "Delete";
}
