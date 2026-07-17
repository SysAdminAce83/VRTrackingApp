using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Orchestrates an automated remediation: resolve the plan, pick the right
/// provider, run CHECK or INSTALL, then record the outcome to the job, the
/// vulnerability status, and the audit trail (evidence).
/// </summary>
public class RemediationEngine
{
    private readonly VRTrackingAppContext _db;
    private readonly PatchIdentifierParser _parser;
    private readonly IEnumerable<IRemediationProvider> _providers;
    private readonly RemediationOptions _options;
    private readonly ILogger<RemediationEngine> _log;

    public RemediationEngine(
        VRTrackingAppContext db,
        PatchIdentifierParser parser,
        IEnumerable<IRemediationProvider> providers,
        IOptions<RemediationOptions> options,
        ILogger<RemediationEngine> log)
    {
        _db = db;
        _parser = parser;
        _providers = providers;
        _options = options.Value;
        _log = log;
    }

    /// <summary>Creates a queued job for an instance and returns its id.</summary>
    public async Task<int> CreateJobAsync(int instanceId, string jobType, int? userId)
    {
        var inst = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h!.Asset)
            .FirstOrDefaultAsync(i => i.Id == instanceId)
            ?? throw new InvalidOperationException($"Vulnerability instance {instanceId} not found.");

        var plan = _parser.Resolve(inst);
        var os = inst.AssetHost?.OperatingSystem ?? inst.AssetHost?.Asset?.OperatingSystem ?? "";
        var isCritical = string.Equals(inst.AssetHost?.Asset?.BiaCriticality, "Critical", StringComparison.OrdinalIgnoreCase);

        var job = new RemediationJob
        {
            VulnerabilityInstanceId = instanceId,
            JobType = jobType,
            State = RemediationJobStates.Queued,
            TargetHost = inst.AssetHost?.HostName ?? inst.AssetHost?.IpAddress,
            OperatingSystem = os,
            PatchId = plan.PatchId,
            IsCriticalAsset = isCritical,
            RequestedByUserId = userId,
            Log = $"Queued {jobType}. Plan: {plan.Kind} — {plan.Reason}"
        };
        _db.RemediationJobs.Add(job);
        await _db.SaveChangesAsync();
        return job.Id;
    }

    /// <summary>Executes a previously queued job (called by the background worker).</summary>
    public async Task RunJobAsync(int jobId, CancellationToken ct)
    {
        var job = await _db.RemediationJobs
            .Include(j => j.VulnerabilityInstance).ThenInclude(i => i!.VulnerabilityFinding)
            .Include(j => j.VulnerabilityInstance).ThenInclude(i => i!.AssetHost).ThenInclude(h => h!.Asset)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return;

        var inst = job.VulnerabilityInstance!;
        var host = job.TargetHost ?? "";
        var os = job.OperatingSystem ?? "";
        var plan = _parser.Resolve(inst);

        job.State = RemediationJobStates.Running;
        job.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        try
        {
            // Not automatable → stop early with guidance.
            if (plan.Kind == RemediationKind.Manual)
            {
                await Finish(job, inst, RemediationJobStates.NotApplicable,
                    "Not automatable — manual review required.", plan.Reason, ct);
                return;
            }

            // Safety gate: never auto-install on a Critical asset.
            if (job.JobType == RemediationJobTypes.Install && job.IsCriticalAsset && _options.TreatCriticalAsApprovalRequired)
            {
                await Finish(job, inst, RemediationJobStates.RequiresApproval,
                    "Critical asset — install blocked. Schedule downtime and approve before installing.",
                    "BIA criticality = Critical. Per policy, installs require an approved maintenance window.", ct);
                return;
            }

            var provider = ResolveProvider(os);
            var ctx = new RemediationContext(host, inst.AssetHost?.IpAddress, os, plan.PatchId, job.IsCriticalAsset)
            {
                SourceKey = inst.VulnerabilityFinding?.PluginId.ToString(),
                Registry = plan.Registry
            };
            AppendLog(job, $"Provider: {provider.Name} | Mode: {_options.Mode} | Plan: {plan.Kind}");

            var result = job.JobType == RemediationJobTypes.Install
                ? await provider.InstallAsync(ctx, ct)
                : await provider.CheckAsync(ctx, ct);

            // A registry fix that still needs microcode/BIOS is NOT fully remediated,
            // so don't auto-close the vulnerability in that case.
            var partialOnly = plan.Kind == RemediationKind.RegistryConfig && plan.Registry?.RequiresMicrocode == true;
            var markFixed = result.Success
                && job.JobType == RemediationJobTypes.Install
                && result.State == RemediationJobStates.Succeeded
                && !partialOnly;

            var summary = partialOnly && job.JobType == RemediationJobTypes.Install && result.Success
                ? result.Summary + " NOTE: CPU microcode/BIOS update is also required — not fully remediated until firmware is updated."
                : result.Summary;

            await Finish(job, inst, result.State, summary, result.Log, ct, markFixed);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Remediation job {JobId} failed", jobId);
            await Finish(job, inst, RemediationJobStates.Failed, "Remediation errored.", ex.ToString(), ct, false);
        }
    }

    private IRemediationProvider ResolveProvider(string os)
    {
        if (!_options.IsLive)
            return _providers.First(p => p.Name == "Simulated");

        return _providers.FirstOrDefault(p => p.Name != "Simulated" && p.CanHandle(os))
               ?? _providers.First(p => p.Name == "Simulated");
    }

    private async Task Finish(RemediationJob job, VulnerabilityInstance inst, string state,
        string summary, string log, CancellationToken ct, bool applyStatus = true)
    {
        AppendLog(job, log);
        job.State = state;
        job.ResultSummary = summary;
        job.CompletedAt = DateTime.UtcNow;

        // Reflect successful installs on the vulnerability + drop a full audit record.
        var actionLabel = job.JobType == RemediationJobTypes.Install ? "Auto-remediate" : "Auto-check";
        if (applyStatus && job.JobType == RemediationJobTypes.Install && state == RemediationJobStates.Succeeded)
            inst.Status = "Fixed";

        _db.RemediationActions.Add(new RemediationAction
        {
            VulnerabilityInstanceId = inst.Id,
            Action = actionLabel,
            Status = state,
            PerformedByUserId = job.RequestedByUserId,
            Comments = $"[{job.TargetHost}] {summary}",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }

    private static void AppendLog(RemediationJob job, string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return;
        job.Log = string.IsNullOrEmpty(job.Log) ? line : job.Log + "\n" + line;
    }
}
