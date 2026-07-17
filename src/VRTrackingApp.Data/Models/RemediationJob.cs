using System;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Records an automated remediation run against a single <see cref="VulnerabilityInstance"/>.
/// A "Check" job is read-only (is the patch installed / available / reboot pending);
/// an "Install" job attempts to remediate. Every step is logged for audit/evidence.
/// </summary>
public class RemediationJob
{
    public int Id { get; set; }

    public int VulnerabilityInstanceId { get; set; }
    public VulnerabilityInstance? VulnerabilityInstance { get; set; }

    /// <summary>Check | Install</summary>
    public string JobType { get; set; } = RemediationJobTypes.Check;

    /// <summary>Queued | Running | Succeeded | Failed | NotApplicable | RequiresApproval</summary>
    public string State { get; set; } = RemediationJobStates.Queued;

    public string? TargetHost { get; set; }
    public string? OperatingSystem { get; set; }
    public string? PatchId { get; set; }
    public bool IsCriticalAsset { get; set; }

    /// <summary>Short human-readable outcome, e.g. "KB5094122 MISSING (reboot not pending)".</summary>
    public string? ResultSummary { get; set; }

    /// <summary>Full multi-line step log.</summary>
    public string Log { get; set; } = "";

    public int? RequestedByUserId { get; set; }
    public UserAccount? RequestedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public static class RemediationJobTypes
{
    public const string Check = "Check";
    public const string Install = "Install";
}

public static class RemediationJobStates
{
    public const string Queued = "Queued";
    public const string Running = "Running";
    public const string Succeeded = "Succeeded";
    public const string Failed = "Failed";
    public const string NotApplicable = "NotApplicable";
    public const string RequiresApproval = "RequiresApproval";
}
