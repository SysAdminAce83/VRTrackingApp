using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.NVD;
using VRTrackingApp.Web.Services.NVD.Models;

namespace VRTrackingApp.Web.Services.NVD;

public class NvdSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NvdSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(12);
    private readonly TimeSpan _firstRunDelay = TimeSpan.FromMinutes(5);

    public NvdSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<NvdSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NVD Sync Background Service starting");

        await Task.Delay(_firstRunDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during NVD sync");
            }

            try
            {
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("NVD Sync Background Service stopping");
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VRTrackingAppContext>();
        var enrichment = scope.ServiceProvider.GetRequiredService<INvdEnrichmentService>();
        var nvdService = scope.ServiceProvider.GetRequiredService<INvdService>();

        _logger.LogInformation("Starting NVD delta sync");

        var lastSync = await GetLastSyncAsync(db);
        var since = lastSync ?? DateTime.UtcNow.AddDays(-30);

        _logger.LogDebug("Fetching NVD CVEs since {Since}", since);

        var pubStartDate = since.ToString("yyyy-MM-ddTHH:mm:ss");
        var pubEndDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        var response = await nvdService.GetCvesByDateRangeAsync(pubStartDate, pubEndDate, 0, 200, ct);

        if (response == null || response.Vulnerabilities == null || response.Vulnerabilities.Length == 0)
        {
            _logger.LogInformation("No new NVD CVEs since last sync");
            await UpdateLastSyncAsync(db, DateTime.UtcNow);
            return;
        }

        _logger.LogInformation("Found {Count} NVD CVEs since last sync", response.Vulnerabilities.Length);

        var allCveIds = response.Vulnerabilities
            .Select(v => v.Cve.Id)
            .Distinct()
            .ToList();

        _logger.LogDebug("Processing {Count} unique CVEs from NVD", allCveIds.Count);

        var existingCves = await db.VulnerabilityFindings
            .Where(f => f.Cve != null && allCveIds.Contains(f.Cve))
            .Select(f => f.Cve!)
            .ToHashSetAsync(ct);

        var newCveIds = allCveIds.Where(id => !existingCves.Contains(id)).ToList();

        _logger.LogInformation("Found {NewCount} new CVEs, {ExistingCount} already tracked", newCveIds.Count, existingCves.Count);

        int enriched = 0;
        int created = 0;

        foreach (var nvdVuln in response!.Vulnerabilities ?? Array.Empty<NvdVulnerability>())
        {
            try
            {
                var cve = nvdVuln.Cve;
                var enrichmentData = await enrichment.EnrichByCveAsync(cve.Id, ct);
                if (enrichmentData == null) continue;

                var existingFinding = await db.VulnerabilityFindings
                    .FirstOrDefaultAsync(f => f.Cve == cve.Id, ct);

                if (existingFinding != null)
                {
                    ApplyEnrichmentToFinding(existingFinding, enrichmentData);
                    enriched++;
                }
                else if (newCveIds.Contains(cve.Id))
                {
                    var newFinding = CreateFindingFromNvd(cve, enrichmentData);
                    db.VulnerabilityFindings.Add(newFinding);
                    created++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process NVD CVE {CveId}", nvdVuln.Cve.Id);
            }
        }

        await db.SaveChangesAsync(ct);
        await UpdateLastSyncAsync(db, DateTime.UtcNow);

        _logger.LogInformation("NVD sync complete. Created {Created} new findings, Enriched {Enriched} existing findings", created, enriched);
    }

    private VulnerabilityFinding CreateFindingFromNvd(NvdCve cve, NvdEnrichmentData enrichment)
    {
        return new VulnerabilityFinding
        {
            PluginId = 0,
            PluginName = enrichment.Title ?? enrichment.CveId ?? "NVD Finding",
            Cve = enrichment.CveId,
            Severity = enrichment.CvssV31BaseSeverity ?? "Info",
            Synopsis = enrichment.Description,
            Description = enrichment.Description,
            CvssV3BaseScore = enrichment.CvssV31BaseScore,
            CvssV3TemporalScore = enrichment.CvssV31TemporalScore,
            CvssV2BaseScore = enrichment.CvssV2BaseScore,
            References = enrichment.References != null
                ? string.Join("|", enrichment.References.Select(r => r.Url ?? ""))
                : null,
            CreatedAt = DateTime.UtcNow,
            LastEnrichedAt = DateTime.UtcNow
        };
    }

    private void ApplyEnrichmentToFinding(VulnerabilityFinding finding, NvdEnrichmentData enrichment)
    {
        finding.PluginName = enrichment.Title ?? enrichment.CveId ?? finding.PluginName;
        finding.Severity = enrichment.CvssV31BaseSeverity ?? finding.Severity;
        finding.Description = enrichment.Description ?? finding.Description;
        finding.Synopsis = enrichment.Description ?? finding.Synopsis;
        finding.CvssV3BaseScore = enrichment.CvssV31BaseScore ?? finding.CvssV3BaseScore;
        finding.CvssV3TemporalScore = enrichment.CvssV31TemporalScore ?? finding.CvssV3TemporalScore;
        finding.CvssV2BaseScore = enrichment.CvssV2BaseScore ?? finding.CvssV2BaseScore;
        finding.References = enrichment.References != null
            ? string.Join("|", enrichment.References.Select(r => r.Url ?? ""))
            : finding.References;
        finding.LastEnrichedAt = DateTime.UtcNow;
    }

    private async Task<DateTime?> GetLastSyncAsync(VRTrackingAppContext db)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "NVD_LastSync");
        if (setting != null && DateTime.TryParse(setting.Value, out var dt))
            return dt;
        return null;
    }

    private async Task UpdateLastSyncAsync(VRTrackingAppContext db, DateTime syncTime)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "NVD_LastSync");
        if (setting == null)
        {
            setting = new AppSetting { Key = "NVD_LastSync", Value = syncTime.ToString("O") };
            db.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = syncTime.ToString("O");
        }
        await db.SaveChangesAsync();
    }
}