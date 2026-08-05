using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.MSRC;
using VRTrackingApp.Web.Services.MSRC.Models;

namespace VRTrackingApp.Web.Services.MSRC;

public class MsrcSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MsrcSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(24);
    private readonly TimeSpan _firstRunDelay = TimeSpan.FromMinutes(5);

    public MsrcSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<MsrcSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MSRC Sync Background Service starting");

        // Initial delay to let app start up
        await Task.Delay(_firstRunDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MSRC sync");
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

        _logger.LogInformation("MSRC Sync Background Service stopping");
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<VRTrackingAppContext>();
        var enrichment = scope.ServiceProvider.GetRequiredService<IVulnerabilityEnrichmentService>();
        var msrcService = scope.ServiceProvider.GetRequiredService<IMsrcService>();

        _logger.LogInformation("Starting MSRC delta sync");

        // Get last sync time from database (or use a setting table)
        var lastSync = await GetLastSyncAsync(db);
        var since = lastSync ?? DateTime.UtcNow.AddDays(-30); // Default to 30 days back

        _logger.LogDebug("Fetching MSRC updates since {Since}", since);

        // Fetch updates since last sync using OData filter
        var filter = $"InitialReleaseDate gt {since:yyyy-MM-ddTHH:mm:ssZ}";
        var updates = await msrcService.GetUpdatesAsync(filter, ct);

        if (updates.Length == 0)
        {
            _logger.LogInformation("No new MSRC updates since last sync");
            await UpdateLastSyncAsync(db, DateTime.UtcNow);
            return;
        }

        _logger.LogInformation("Found {Count} new/updated MSRC advisories", updates.Length);

        // Collect all CVEs from updates
        var allCves = updates
            .Where(u => u.Cves != null)
            .SelectMany(u => u.Cves!)
            .Distinct()
            .ToList();

        _logger.LogDebug("Processing {Count} unique CVEs", allCves.Count);

        // Find findings in our database that match these CVEs
        var findingsToEnrich = await db.VulnerabilityFindings
            .Where(f => f.Cve != null && allCves.Contains(f.Cve))
            .ToListAsync(ct);

        _logger.LogInformation("Enriching {Count} existing findings", findingsToEnrich.Count);

        int enriched = 0;
        foreach (var finding in findingsToEnrich)
        {
            try
            {
                var enrichmentData = await enrichment.EnrichByCVEAsync(finding.Cve!, ct);
                if (enrichmentData != null)
                {
                    ApplyEnrichment(finding, enrichmentData);
                    enriched++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to enrich finding {CVE}", finding.Cve);
            }
        }

        await db.SaveChangesAsync(ct);
        await UpdateLastSyncAsync(db, DateTime.UtcNow);

        _logger.LogInformation("MSRC sync complete. Enriched {Enriched} of {Total} findings", enriched, findingsToEnrich.Count);
    }

    private void ApplyEnrichment(VulnerabilityFinding finding, MSRCEnrichmentData enrichment)
    {
        finding.MicrosoftAdvisoryId = enrichment.MicrosoftAdvisoryId;
        finding.MicrosoftBulletinId = enrichment.MicrosoftBulletinId;
        finding.KBNumbers = enrichment.KBNumbers != null ? string.Join(",", enrichment.KBNumbers) : null;
        finding.PatchDownloadUrls = enrichment.PatchDownloadUrls != null ? string.Join(",", enrichment.PatchDownloadUrls) : null;
        finding.RequiresReboot = enrichment.RequiresReboot;
        finding.SupersededBy = enrichment.SupersededBy;
        finding.ExploitabilityAssessment = enrichment.ExploitabilityAssessment;
        finding.MicrosoftReleaseDate = enrichment.MicrosoftReleaseDate?.DateTime;
        finding.AffectedProducts = enrichment.AffectedProducts != null
            ? System.Text.Json.JsonSerializer.Serialize(enrichment.AffectedProducts)
            : null;
        finding.Workaround = enrichment.Workaround;
        finding.FAQUrl = enrichment.FAQUrl;
        finding.CVRFId = enrichment.CVRFId;
        finding.CSAFId = enrichment.CSAFId;
        finding.LastEnrichedAt = DateTime.UtcNow;
    }

    private async Task<DateTime?> GetLastSyncAsync(VRTrackingAppContext db)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "MSRC_LastSync");
        if (setting != null && DateTime.TryParse(setting.Value, out var dt))
            return dt;
        return null;
    }

    private async Task UpdateLastSyncAsync(VRTrackingAppContext db, DateTime syncTime)
    {
        var setting = await db.AppSettings.FirstOrDefaultAsync(s => s.Key == "MSRC_LastSync");
        if (setting == null)
        {
            setting = new AppSetting { Key = "MSRC_LastSync", Value = syncTime.ToString("O") };
            db.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = syncTime.ToString("O");
        }
        await db.SaveChangesAsync();
    }
}