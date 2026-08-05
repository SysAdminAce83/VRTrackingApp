using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.NVD.Models;

namespace VRTrackingApp.Web.Services.NVD;

public interface INvdEnrichmentService
{
    Task<NvdEnrichmentData?> EnrichByCveAsync(string cveId, CancellationToken ct = default);
    Task BulkEnrichAsync(IEnumerable<string> cveIds, CancellationToken ct = default);
}

public class NvdEnrichmentService : INvdEnrichmentService
{
    private readonly INvdService _nvdService;
    private readonly ILogger<NvdEnrichmentService> _logger;
    private readonly Dictionary<string, NvdEnrichmentData> _cache = new();

    public NvdEnrichmentService(INvdService nvdService, ILogger<NvdEnrichmentService> logger)
    {
        _nvdService = nvdService;
        _logger = logger;
    }

    public async Task<NvdEnrichmentData?> EnrichByCveAsync(string cveId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cveId))
            return null;

        if (_cache.TryGetValue(cveId, out var cached))
            return cached;

        _logger.LogDebug("Enriching vulnerability from NVD: {CveId}", cveId);

        var cve = await _nvdService.GetCveByIdAsync(cveId, ct);
        if (cve == null)
        {
            _logger.LogWarning("NVD returned no data for CVE: {CveId}", cveId);
            return null;
        }

        var enrichment = MapToEnrichmentData(cve);
        _cache[cveId] = enrichment;
        return enrichment;
    }

    public async Task BulkEnrichAsync(IEnumerable<string> cveIds, CancellationToken ct = default)
    {
        var distinctCves = cveIds.Distinct().ToList();
        _logger.LogInformation("Bulk enriching {Count} CVEs from NVD", distinctCves.Count);

        var tasks = distinctCves.Select(cve => EnrichByCveAsync(cve, ct));
        await Task.WhenAll(tasks);
    }

    private NvdEnrichmentData MapToEnrichmentData(NvdCve cve)
    {
        var description = cve.Descriptions?
            .FirstOrDefault(d => d.Lang == "en")?.Value
            ?? cve.Descriptions?.FirstOrDefault()?.Value;

        var cvssV31 = cve.Metrics?.CvssMetricV31?
            .FirstOrDefault(m => m.Type == "Primary");
        var cvssV30 = cve.Metrics?.CvssMetricV30?
            .FirstOrDefault(m => m.Type == "Primary");
        var cvssV2 = cve.Metrics?.CvssMetricV2?
            .FirstOrDefault(m => m.Type == "Primary");

        return new NvdEnrichmentData
        {
            CveId = cve.Id,
            Title = cve.CisaVulnerabilityName,
            Description = description,
            CvssV31BaseScore = cvssV31?.Score ?? cvssV31?.CvssData?.BaseScore,
            CvssV31TemporalScore = cvssV31?.ExploitabilityScore,
            CvssV30BaseScore = cvssV30?.Score ?? cvssV30?.CvssData?.BaseScore,
            CvssV2BaseScore = cvssV2?.Score,
            CvssV31Vector = cvssV31?.VectorString ?? cvssV31?.CvssData?.VectorString,
            CvssV31BaseSeverity = cvssV31?.BaseSeverity ?? cvssV31?.CvssData?.BaseSeverity,
            VulnStatus = cve.VulnStatus,
            CisaVulnerabilityName = cve.CisaVulnerabilityName,
            Published = cve.Published,
            LastModified = cve.LastModified,
            SourceIdentifier = cve.SourceIdentifier,
            References = cve.References,
            CweIds = cve.Weaknesses?
                .SelectMany(w => w.Description ?? Array.Empty<NvdWeaknessDescription>())
                .Where(d => d.Lang == "en")
                .Select(d => d.Value)
                .Distinct()
                .ToArray()
        };
    }
}