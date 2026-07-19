namespace VRTrackingApp.Data.Models;

/// <summary>High-level lifecycle status of an exception (owned by ExceptionWorkflowService).</summary>
public enum ExceptionStatus
{
    Detected,
    UnderReview,
    ExceptionRequested,
    PendingTechnicalApproval,   // Infrastructure Manager OR Network Manager (routed by type)
    PendingManagerApproval,    // Risk Committee
    PendingSecurityApproval,   // CISO
    NeedMoreInfo,
    Rejected,
    Approved,
    ActiveException,
    ReviewDue,
    Renewed,
    Expired,
    Closed
}

/// <summary>A stage in the approval chain.</summary>
public enum ApprovalStage
{
    Technical,      // Infrastructure Manager OR Network Manager (routed by type)
    Manager,        // Risk Committee
    Security        // CISO
}

public static class ExceptionStatusLabels
{
    public static string For(ExceptionStatus status) => status switch
    {
        ExceptionStatus.Detected => "Detected",
        ExceptionStatus.UnderReview => "Under Review",
        ExceptionStatus.ExceptionRequested => "Exception Requested",
        ExceptionStatus.PendingTechnicalApproval => "Pending Technical Approval",
        ExceptionStatus.PendingManagerApproval => "Pending Manager Approval",
        ExceptionStatus.PendingSecurityApproval => "Pending Security Approval",
        ExceptionStatus.NeedMoreInfo => "Need More Info",
        ExceptionStatus.Rejected => "Rejected",
        ExceptionStatus.Approved => "Approved",
        ExceptionStatus.ActiveException => "Active Exception",
        ExceptionStatus.ReviewDue => "Review Due",
        ExceptionStatus.Renewed => "Renewed",
        ExceptionStatus.Expired => "Expired",
        ExceptionStatus.Closed => "Closed",
        _ => status.ToString()
    };
}

public enum ApprovalDecision
{
    Pending,
    Approved,
    Rejected,
    NeedMoreInfo,

    // ---- Scan ingestion / deduplication (Scenario 12) ----
    ScanIngested,
    ScanDuplicateDetected,
    ScanAlreadyProcessing,
    ScanAdditionalFindings,
    ScanRejected,
    ScanMerged
}

/// <summary>Section 2 — why the vulnerability is not fixable.</summary>
public enum NonFixableReason
{
    VendorPatchNotAvailable,
    LegacyApplicationDependency,
    EndOfLifeOperatingSystem,
    BusinessApplicationDependency,
    ThirdPartySoftwareLimitation,
    HardwareLimitation,
    OperationalConstraint,
    DowntimeNotApproved,
    RegulatoryRequirement,
    UnsupportedConfiguration,
    FalsePositive,
    AcceptedBusinessRisk,
    Other
}

/// <summary>Section 5 — likelihood axis.</summary>
public enum Likelihood { VeryLow, Low, Medium, High, Critical }

/// <summary>Section 5 — impact axis.</summary>
public enum ImpactLevel { Low, Medium, High, Critical }

/// <summary>Section 5 — computed overall risk.</summary>
public enum RiskLevel { Low, Medium, High, Critical }

/// <summary>Section 7 — exploitability.</summary>
public enum Exploitability { PublicExploit, PoCAvailable, NoExploit, Unknown }

/// <summary>Section 8 — internet exposure.</summary>
public enum InternetExposure { InternetFacing, InternalOnly, VpnOnly, Dmz, AirGapped }

/// <summary>Section 10 — mitigation status.</summary>
public enum MitigationStatus { Planned, Pending, Implemented }

/// <summary>Section 14 — periodic review outcome.</summary>
public enum ReviewOutcome { Pending, Renewed, Closed }

/// <summary>Section 11 — evidence document type.</summary>
public enum EvidenceType
{
    FirewallScreenshot,
    EdrScreenshot,
    VendorEmail,
    MicrosoftKb,
    RiskAssessmentPdf,
    ArchitectureDiagram,
    ChangeRecord,
    CabApproval,
    PowerShellOutput,
    RegistryExport,
    ConfigurationScreenshot,
    NetworkDiagram,
    PatchTestResult,
    ApplicationOwnerEmail,
    Other
}

/// <summary>Notification event types (in-app + email).</summary>
public enum NotificationType
{
    NewExceptionRequest,
    ApprovalRequired,
    RequestRejected,
    ExceptionApproved,
    ExceptionExpiring,
    ExceptionExpired,
    ReviewDue,
    EvidenceMissing,
    MitigationOverdue,
    NeedMoreInfo,

    // ---- Scan ingestion / deduplication (Scenario 12) ----
    ScanIngested,
    ScanDuplicateDetected,
    ScanAlreadyProcessing,
    ScanAdditionalFindings,
    ScanRejected,
    ScanMerged
}

/// <summary>Reason an exception was closed.</summary>
public enum ClosedReason { Patched, Mitigated, FalsePositive, RiskRemoved }

/// <summary>
/// Static catalogs used by the request form (kept in code to avoid extra lookup tables).
/// </summary>
public static class ExceptionCatalogs
{
    /// <summary>Section 9 — existing security controls.</summary>
    public static readonly string[] SecurityControls =
    {
        "Firewall", "EDR", "MFA", "IPS", "IDS", "Application Control",
        "Network Segmentation", "SIEM Monitoring", "WAF", "AV",
        "Disk Encryption", "Least Privilege", "Jump Server", "Conditional Access"
    };
}

