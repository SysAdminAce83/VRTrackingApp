using System;

namespace VRTrackingApp.Data.Models;

/// <summary>Section 12 — one stage in the exception approval chain.</summary>
public class ExceptionApprovalStep
{
    public int Id { get; set; }
    public int ExceptionRecordId { get; set; }
    public ExceptionRecord? ExceptionRecord { get; set; }

    public int StepOrder { get; set; }
    public ApprovalStage Stage { get; set; }
    /// <summary>Role required to action this step (e.g. InfrastructureManager, RiskCommittee, CISO).</summary>
    public string RequiredRole { get; set; } = default!;

    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;
    public int? DecisionByUserId { get; set; }
    public UserAccount? DecisionBy { get; set; }
    public DateTime? DecisionAt { get; set; }
    public string? Comment { get; set; }
}
