using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.NVD;
using VRTrackingApp.Web.Services.NVD.Models;

namespace VRTrackingApp.Web.Controllers;

public class NvdController : Controller
{
    private readonly INvdService _nvdService;
    private readonly INvdEnrichmentService _enrichmentService;
    private readonly VRTrackingAppContext _db;

    public NvdController(INvdService nvdService, INvdEnrichmentService enrichmentService, VRTrackingAppContext db)
    {
        _nvdService = nvdService;
        _enrichmentService = enrichmentService;
        _db = db;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "NVD Integration";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword is required");

        var startIndex = (page - 1) * pageSize;
        var response = await _nvdService.GetCveByKeywordAsync(keyword, startIndex, pageSize);

        if (response == null)
            return StatusCode(502, "NVD service unavailable");

        var results = response.Vulnerabilities?.Select(v => new NvdSearchResultDto
        {
            CveId = v.Cve.Id,
            Title = v.Cve.CisaVulnerabilityName,
            Description = v.Cve.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value,
            Published = v.Cve.Published,
            LastModified = v.Cve.LastModified,
            VulnStatus = v.Cve.VulnStatus,
            CvssV31BaseScore = v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault(m => m.Type == "Primary")?.Score
                ?? v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.Score,
            CvssV31BaseSeverity = v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault(m => m.Type == "Primary")?.BaseSeverity
                ?? v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData?.BaseSeverity,
            References = v.Cve.References?.Select(r => r.Url).Where(u => u != null).ToList()
        }).ToList();

        return Json(new NvdSearchResponseDto
        {
            TotalResults = response.TotalResults,
            ResultsPerPage = response.ResultsPerPage,
            StartIndex = response.StartIndex,
            Results = results ?? new List<NvdSearchResultDto>()
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetCve(string cveId)
    {
        if (string.IsNullOrWhiteSpace(cveId))
            return BadRequest("CVE ID is required");

        var cve = await _nvdService.GetCveByIdAsync(cveId.Trim());
        if (cve == null)
            return NotFound($"CVE {cveId} not found in NVD");

        var enrichment = await _enrichmentService.EnrichByCveAsync(cveId.Trim());

        var dto = new NvdCveDetailDto
        {
            CveId = cve.Id,
            Title = enrichment?.Title ?? cve.CisaVulnerabilityName,
            Description = enrichment?.Description ?? cve.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value,
            Published = cve.Published,
            LastModified = cve.LastModified,
            VulnStatus = cve.VulnStatus,
            CvssV31BaseScore = enrichment?.CvssV31BaseScore,
            CvssV31Vector = enrichment?.CvssV31Vector,
            CvssV31BaseSeverity = enrichment?.CvssV31BaseSeverity,
            CvssV30BaseScore = enrichment?.CvssV30BaseScore,
            CvssV2BaseScore = enrichment?.CvssV2BaseScore,
            CweIds = enrichment?.CweIds,
            References = enrichment?.References?.Select(r => new NvdReferenceDto
            {
                Url = r.Url,
                Source = r.Source,
                Tags = r.Tags
            }).ToList()
        };

        return Json(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Sync([FromBody] NvdSyncRequestDto? request)
    {
        var cveId = request?.CveId;
        if (!string.IsNullOrWhiteSpace(cveId))
        {
            var cve = await _nvdService.GetCveByIdAsync(cveId);
            if (cve == null)
                return NotFound($"CVE {cveId} not found in NVD");

            var enrichment = await _enrichmentService.EnrichByCveAsync(cveId);
            if (enrichment == null)
                return StatusCode(502, "Failed to enrich CVE from NVD");

            var existing = await _db.VulnerabilityFindings
                .FirstOrDefaultAsync(f => f.Cve == cveId);

            if (existing != null)
            {
                existing.PluginName = enrichment.Title ?? existing.PluginName;
                existing.Severity = enrichment.CvssV31BaseSeverity ?? existing.Severity;
                existing.Description = enrichment.Description ?? existing.Description;
                existing.Synopsis = enrichment.Description ?? existing.Synopsis;
                existing.CvssV3BaseScore = enrichment.CvssV31BaseScore ?? existing.CvssV3BaseScore;
                existing.CvssV3TemporalScore = enrichment.CvssV31TemporalScore ?? existing.CvssV3TemporalScore;
                existing.CvssV2BaseScore = enrichment.CvssV2BaseScore ?? existing.CvssV2BaseScore;
                existing.References = enrichment.References != null
                    ? string.Join("|", enrichment.References.Select(r => r.Url ?? ""))
                    : existing.References;
                existing.LastEnrichedAt = DateTime.UtcNow;
            }
            else
            {
                var newFinding = new VulnerabilityFinding
                {
                    PluginId = 0,
                    PluginName = enrichment.Title ?? cveId,
                    Cve = cveId,
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
                _db.VulnerabilityFindings.Add(newFinding);
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"CVE {cveId} synced successfully" });
        }

        return BadRequest("CVE ID is required");
    }

    [HttpGet]
    public async Task<IActionResult> ByDateRange(string startDate, string endDate)
    {
        if (string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
            return BadRequest("Start date and end date are required");

        var response = await _nvdService.GetCvesByDateRangeAsync(startDate, endDate);
        if (response == null)
            return StatusCode(502, "NVD service unavailable");

        var results = response.Vulnerabilities?.Select(v => new NvdSearchResultDto
        {
            CveId = v.Cve.Id,
            Title = v.Cve.CisaVulnerabilityName,
            Description = v.Cve.Descriptions?.FirstOrDefault(d => d.Lang == "en")?.Value,
            Published = v.Cve.Published,
            LastModified = v.Cve.LastModified,
            VulnStatus = v.Cve.VulnStatus,
            CvssV31BaseScore = v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault(m => m.Type == "Primary")?.Score
                ?? v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.Score,
            CvssV31BaseSeverity = v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault(m => m.Type == "Primary")?.BaseSeverity
                ?? v.Cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData?.BaseSeverity,
            References = v.Cve.References?.Select(r => r.Url).Where(u => u != null).ToList()
        }).ToList();

        return Json(new NvdSearchResponseDto
        {
            TotalResults = response.TotalResults,
            ResultsPerPage = response.ResultsPerPage,
            StartIndex = response.StartIndex,
            Results = results ?? new List<NvdSearchResultDto>()
        });
    }
}

public class NvdSearchResultDto
{
    public string CveId { get; set; } = default!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Published { get; set; }
    public string? LastModified { get; set; }
    public string? VulnStatus { get; set; }
    public double? CvssV31BaseScore { get; set; }
    public string? CvssV31BaseSeverity { get; set; }
    public List<string>? References { get; set; }
}

public class NvdSearchResponseDto
{
    public int TotalResults { get; set; }
    public int ResultsPerPage { get; set; }
    public int StartIndex { get; set; }
    public List<NvdSearchResultDto> Results { get; set; } = new();
}

public class NvdCveDetailDto
{
    public string CveId { get; set; } = default!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Published { get; set; }
    public string? LastModified { get; set; }
    public string? VulnStatus { get; set; }
    public double? CvssV31BaseScore { get; set; }
    public string? CvssV31Vector { get; set; }
    public string? CvssV31BaseSeverity { get; set; }
    public double? CvssV30BaseScore { get; set; }
    public double? CvssV2BaseScore { get; set; }
    public string[]? CweIds { get; set; }
    public List<NvdReferenceDto>? References { get; set; }
}

public class NvdReferenceDto
{
    public string? Url { get; set; }
    public string? Source { get; set; }
    public string[]? Tags { get; set; }
}

public class NvdSyncRequestDto
{
    public string? CveId { get; set; }
}