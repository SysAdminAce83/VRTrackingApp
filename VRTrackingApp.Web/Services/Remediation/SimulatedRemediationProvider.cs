using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Pretends to talk to a server so the full workflow can be demonstrated and
/// developed without real hosts. Deterministic per host+patch so results are
/// stable. Used whenever Remediation:Mode = "Simulated" (the default).
/// </summary>
public class SimulatedRemediationProvider : IRemediationProvider
{
    public string Name => "Simulated";

    // Handles every OS in simulation mode.
    public bool CanHandle(string operatingSystem) => true;

    public async Task<ProviderStepResult> CheckAsync(RemediationContext ctx, CancellationToken ct)
    {
        await Task.Delay(600, ct); // pretend a remote round-trip

        if (ctx.Registry is not null)
            return SimulateRegistryCheck(ctx);

        var kb = EffectivePatchId(ctx);
        var state = HostState(ctx.Host, kb);
        var log = $"[sim] Connected to {ctx.Host} ({ctx.IpAddress}) [{ctx.OperatingSystem}]\n" +
                  $"[sim] Querying update state for {kb}...";

        return state switch
        {
            0 => ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                    $"{kb} is INSTALLED and verified. No action needed.",
                    log + $"\n[sim] {kb} found in installed updates. OK."),
            1 => ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                    $"{kb} is MISSING but AVAILABLE in Windows Update (ready to install).",
                    log + $"\n[sim] {kb} not installed.\n[sim] {kb} is staged/available and ready to install."),
            2 => ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                    $"{kb} is INSTALLED but a REBOOT is pending to complete it.",
                    log + $"\n[sim] {kb} installed; reboot pending flag is set."),
            _ => ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                    $"{kb} is MISSING and NOT yet available on the host (patch not staged).",
                    log + $"\n[sim] {kb} not installed and not present in the available list.")
        };
    }

    public async Task<ProviderStepResult> InstallAsync(RemediationContext ctx, CancellationToken ct)
    {
        if (ctx.Registry is not null)
        {
            await Task.Delay(900, ct);
            return SimulateRegistrySet(ctx);
        }

        var kb = EffectivePatchId(ctx);
        var log = $"[sim] Connected to {ctx.Host}.\n[sim] Triggering Windows Update detection scan...";
        await Task.Delay(800, ct);

        var state = HostState(ctx.Host, kb);
        if (state == 0)
            return ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                $"{kb} was already installed — nothing to do.",
                log + $"\n[sim] {kb} already installed.");

        if (state == 3)
            return ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
                $"{kb} is not available on the host yet (Tanium hasn't staged it). Try again later.",
                log + $"\n[sim] {kb} not available to install.");

        log += $"\n[sim] {kb} found and ready.\n[sim] Downloading...";
        await Task.Delay(1000, ct);
        log += "\n[sim] Installing...";
        await Task.Delay(1200, ct);

        var reboot = state == 2 || (ctx.Host.Length % 2 == 0);
        return ProviderStepResult.Ok(RemediationJobStates.Succeeded,
            reboot ? $"{kb} INSTALLED successfully. Reboot required to finish."
                   : $"{kb} INSTALLED successfully and verified.",
            log + $"\n[sim] Install result: SUCCESS (reboot required: {reboot}).");
    }

    private static string EffectivePatchId(RemediationContext ctx) =>
        string.IsNullOrWhiteSpace(ctx.PatchId)
            ? $"KB{5000000 + Math.Abs((ctx.Host + ctx.OperatingSystem + ctx.SourceKey).GetHashCode()) % 900000}"
            : ctx.PatchId;

    // 0=installed, 1=missing-available, 2=installed-reboot-pending, 3=missing-not-available
    private static int HostState(string host, string kb) =>
        Math.Abs((host + kb).GetHashCode()) % 4;

    private static ProviderStepResult SimulateRegistryCheck(RemediationContext ctx)
    {
        var pb = ctx.Registry!;
        var log = $"[sim] Connected to {ctx.Host} [{ctx.OperatingSystem}]\n[sim] Reading registry for '{pb.Name}'...";
        var bad = new List<string>();
        foreach (var s in pb.Settings)
        {
            // Deterministically mark ~half the settings as not-yet-set for the demo.
            var ok = Math.Abs((ctx.Host + s.Name).GetHashCode()) % 2 == 0;
            var current = ok ? s.ExpectedDisplay : "0";
            log += $"\n[sim] {s.Path}\\{s.Name}: current={current} expected={s.ExpectedDisplay} => {(ok ? "OK" : "MISMATCH")}";
            if (!ok) bad.Add(s.Name);
        }

        var summary = bad.Count == 0
            ? $"Mitigation is CORRECTLY configured ({pb.Settings.Count} registry value(s) match)."
            : $"Mitigation NOT fully applied: {bad.Count} of {pb.Settings.Count} registry value(s) incorrect ({string.Join(", ", bad)}).";
        if (pb.RequiresMicrocode)
            summary += " CPU microcode/BIOS support must also be present.";

        return ProviderStepResult.Ok(RemediationJobStates.Succeeded, summary, log);
    }

    private static ProviderStepResult SimulateRegistrySet(RemediationContext ctx)
    {
        var pb = ctx.Registry!;
        var log = $"[sim] Connected to {ctx.Host}.\n[sim] Applying {pb.Settings.Count} registry value(s)...";
        foreach (var s in pb.Settings)
            log += $"\n[sim] SET {s.Path}\\{s.Name} = {s.ExpectedDisplay} ({s.Type})";

        var summary = $"Applied {pb.Settings.Count} registry value(s) for '{pb.Name}'.";
        if (pb.RequiresReboot) summary += " Reboot required to take effect.";

        return ProviderStepResult.Ok(RemediationJobStates.Succeeded, summary, log);
    }
}
