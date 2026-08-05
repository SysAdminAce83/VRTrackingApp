namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Options bound from the "Remediation" configuration section.
/// In "Simulated" mode nothing touches real servers (used for dev/demo).
/// In "Live" mode the OS-specific providers connect to the target host.
/// </summary>
public class RemediationOptions
{
    public const string SectionName = "Remediation";

    /// <summary>Simulated | Live</summary>
    public string Mode { get; set; } = "Simulated";

    /// <summary>When true, Install on a Critical asset is blocked and marked RequiresApproval.</summary>
    public bool TreatCriticalAsApprovalRequired { get; set; } = true;

    public WinRmOptions WinRm { get; set; } = new();

    public bool IsLive => string.Equals(Mode, "Live", StringComparison.OrdinalIgnoreCase);
}

public class WinRmOptions
{
    public bool UseSsl { get; set; } = true;
    public int Port { get; set; } = 5986;

    /// <summary>Optional explicit account (DOMAIN\\user). If empty, the app's own identity (gMSA/service account) is used via Kerberos.</summary>
    public string? AuthUser { get; set; }

    /// <summary>Name of an environment variable holding the password for AuthUser. The password is never stored in appsettings.</summary>
    public string AuthPasswordEnvVar { get; set; } = "VRT_REMEDIATION_PWD";
}

/// <summary>Kind of remediation the engine believes a finding needs.</summary>
public enum RemediationKind
{
    WindowsKb,
    RegistryConfig,
    LinuxPackage,
    Manual
}

/// <summary>The resolved remediation plan for a vulnerability instance.</summary>
public record RemediationPlan(RemediationKind Kind, string? PatchId, string Reason)
{
    /// <summary>Set when Kind == RegistryConfig.</summary>
    public RegistryPlaybook? Registry { get; init; }

    /// <summary>Direct download URL for the patch (from MSRC enrichment).</summary>
    public string? PatchDownloadUrl { get; init; }

    /// <summary>All KB numbers from MSRC enrichment (when multiple apply).</summary>
    public string[]? AllKbNumbers { get; init; }
}

/// <summary>Everything a provider needs to act on one host.</summary>
public record RemediationContext(
    string Host,
    string? IpAddress,
    string OperatingSystem,
    string? PatchId,
    bool IsCritical)
{
    /// <summary>Finding-specific key (e.g. plugin id) used only by the simulator to vary demo outcomes. Ignored by live providers.</summary>
    public string? SourceKey { get; init; }

    /// <summary>Registry recipe to check/set (when the finding is a configuration fix).</summary>
    public RegistryPlaybook? Registry { get; init; }

    /// <summary>Direct download URL for the patch (from MSRC enrichment).</summary>
    public string? PatchDownloadUrl { get; init; }

    /// <summary>All KB numbers from MSRC enrichment (when multiple apply).</summary>
    public string[]? AllKbNumbers { get; init; }
}

/// <summary>Result of a provider check/install step.</summary>
public record ProviderStepResult(bool Success, string State, string Summary, string Log)
{
    public static ProviderStepResult Ok(string state, string summary, string log) => new(true, state, summary, log);
    public static ProviderStepResult Fail(string summary, string log) =>
        new(false, VRTrackingApp.Data.Models.RemediationJobStates.Failed, summary, log);
}
