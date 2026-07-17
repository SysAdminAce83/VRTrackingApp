using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Placeholder for real Linux remediation over SSH (check via rpm/dpkg, remediate
/// via yum/dnf/apt, or trigger Tanium). Wired into the engine now so Linux hosts
/// flow through the same UI; in Live mode it reports that the SSH provider still
/// needs to be configured, rather than guessing and running the wrong command.
/// </summary>
public class LinuxSshRemediationProvider : IRemediationProvider
{
    public string Name => "Linux/SSH";

    public bool CanHandle(string operatingSystem) =>
        new[] { "linux", "red hat", "redhat", "rhel", "ubuntu", "centos", "debian", "oracle linux", "suse" }
            .Any(k => operatingSystem.Contains(k, StringComparison.OrdinalIgnoreCase));

    public Task<ProviderStepResult> CheckAsync(RemediationContext ctx, CancellationToken ct) =>
        Task.FromResult(ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
            "Linux live remediation (SSH) is not configured yet — manual action required.",
            $"Host {ctx.Host} is Linux. Configure the SSH provider (credentials + package mapping) to enable automation."));

    public Task<ProviderStepResult> InstallAsync(RemediationContext ctx, CancellationToken ct) =>
        Task.FromResult(ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
            "Linux live remediation (SSH) is not configured yet — manual action required.",
            $"Host {ctx.Host} is Linux. Configure the SSH provider to enable installs."));
}
