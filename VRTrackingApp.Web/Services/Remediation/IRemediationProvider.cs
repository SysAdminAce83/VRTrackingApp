namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// A transport/OS-specific remediator. Implementations: Simulated (dev/demo),
/// WinRM+Windows Update Agent (Windows), SSH (Linux). Tanium's API could be
/// added as another provider later without changing the engine.
/// </summary>
public interface IRemediationProvider
{
    /// <summary>Human-friendly provider name for logs.</summary>
    string Name { get; }

    /// <summary>True if this provider should handle the given operating system.</summary>
    bool CanHandle(string operatingSystem);

    /// <summary>Read-only: is the patch installed / available / reboot pending?</summary>
    Task<ProviderStepResult> CheckAsync(RemediationContext ctx, CancellationToken ct);

    /// <summary>Attempt to install / remediate. Should be safe to call after a Check.</summary>
    Task<ProviderStepResult> InstallAsync(RemediationContext ctx, CancellationToken ct);
}
