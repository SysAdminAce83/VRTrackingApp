using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Compliance;

namespace VRTrackingApp.Web.Controllers;

public class ComplianceController : Controller
{
    private readonly IComplianceControlService _controlService;
    private readonly IFindingComplianceLinkService _linkService;
    private readonly IComplianceReportService _reportService;
    private readonly VRTrackingAppContext _db;

    public ComplianceController(
        IComplianceControlService controlService,
        IFindingComplianceLinkService linkService,
        IComplianceReportService reportService,
        VRTrackingAppContext db)
    {
        _controlService = controlService;
        _linkService = linkService;
        _reportService = reportService;
        _db = db;
    }

    public async Task<IActionResult> Index(string? framework, string? family, string? search, int page = 1)
    {
        ViewData["Title"] = "Compliance Control Library";
        ViewBag.Framework = framework;
        ViewBag.Family = family;
        ViewBag.Search = search;

        var controls = await _controlService.GetAllAsync(framework, family, search);
        var frameworks = await _controlService.GetFrameworksAsync();

        var model = new ComplianceIndexViewModel
        {
            Controls = controls,
            Frameworks = frameworks,
            SearchTerm = search,
            SelectedFramework = framework,
            SelectedFamily = family,
            Page = page
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var control = await _controlService.GetByIdAsync(id);
        if (control == null) return NotFound();

        var links = await _linkService.GetByControlIdAsync(control.Id);
        var summary = await _linkService.GetSummaryAsync(0, CancellationToken.None);

        var model = new ComplianceControlDetailViewModel
        {
            Control = control,
            Links = links,
            Summary = summary
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Map(int findingId)
    {
        var finding = await _db.VulnerabilityFindings.FindAsync(findingId);
        if (finding == null) return NotFound();

        var allControls = await _controlService.GetAllAsync();
        var existingLinks = await _linkService.GetByFindingIdAsync(findingId);
        var existingControlIds = existingLinks.Select(l => l.ComplianceControlId).ToHashSet();

        var model = new ComplianceMapViewModel
        {
            FindingId = findingId,
            Cve = finding.Cve,
            PluginName = finding.PluginName,
            Severity = finding.Severity,
            AllControls = allControls,
            ExistingLinks = existingLinks,
            ExistingControlIds = existingControlIds
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Map(int findingId, List<int> controlIds, string? rationale)
    {
        var existingLinks = await _linkService.GetByFindingIdAsync(findingId);
        var existingControlIdSet = existingLinks.Select(l => l.ComplianceControlId).ToHashSet();

        foreach (var controlId in controlIds)
        {
            if (existingControlIdSet.Contains(controlId)) continue;

            var link = new FindingComplianceLink
            {
                VulnerabilityFindingId = findingId,
                ComplianceControlId = controlId,
                Status = ComplianceStatus.InProgress,
                Rationale = rationale,
                CreatedAt = DateTime.UtcNow
            };
            await _linkService.CreateAsync(link);
        }

        return RedirectToAction("Details", "Vulnerabilities", new { id = findingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLinkStatus(int findingId, int controlId, string status)
    {
        var link = await _linkService.GetLinkAsync(findingId, controlId);
        if (link == null) return NotFound();

        link.Status = Enum.Parse<ComplianceStatus>(status);
        link.UpdatedAt = DateTime.UtcNow;
        await _linkService.UpdateAsync(link);

        return RedirectToAction("Details", "Vulnerabilities", new { id = findingId });
    }

    public async Task<IActionResult> Report(string? framework)
    {
        ViewData["Title"] = "Compliance Report";
        ViewBag.Framework = framework;

        var frameworks = await _controlService.GetFrameworksAsync();
        var summary = framework != null
            ? await _reportService.GetFrameworkSummaryAsync(framework)
            : new FrameworkSummaryDto();
        var coverage = await _reportService.GetControlCoverageAsync(framework);
        var failed = await _reportService.GetFailedControlsAsync(framework);
        var gaps = await _reportService.GetEvidenceGapsAsync(framework);
        var exceptions = await _reportService.GetExceptionSummaryAsync(framework);

        var model = new ComplianceReportViewModel
        {
            Frameworks = frameworks,
            SelectedFramework = framework,
            Summary = summary,
            ControlCoverage = coverage,
            FailedControls = failed,
            EvidenceGaps = gaps,
            ExceptionSummaries = exceptions
        };

        return View(model);
    }
}

public class ComplianceIndexViewModel
{
    public IReadOnlyList<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();
    public IReadOnlyList<Framework> Frameworks { get; set; } = new List<Framework>();
    public string? SearchTerm { get; set; }
    public string? SelectedFramework { get; set; }
    public string? SelectedFamily { get; set; }
    public int Page { get; set; }
}

public class ComplianceControlDetailViewModel
{
    public ComplianceControl Control { get; set; } = default!;
    public IReadOnlyList<FindingComplianceLink> Links { get; set; } = new List<FindingComplianceLink>();
    public ComplianceSummaryDto Summary { get; set; } = new();
}

public class ComplianceMapViewModel
{
    public int FindingId { get; set; }
    public string? Cve { get; set; }
    public string? PluginName { get; set; }
    public string? Severity { get; set; }
    public IReadOnlyList<ComplianceControl> AllControls { get; set; } = new List<ComplianceControl>();
    public IReadOnlyList<FindingComplianceLink> ExistingLinks { get; set; } = new List<FindingComplianceLink>();
    public HashSet<int> ExistingControlIds { get; set; } = new();
}

public class ComplianceReportViewModel
{
    public IReadOnlyList<Framework> Frameworks { get; set; } = new List<Framework>();
    public string? SelectedFramework { get; set; }
    public FrameworkSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ControlCoverageDto> ControlCoverage { get; set; } = new List<ControlCoverageDto>();
    public IReadOnlyList<FailedControlDto> FailedControls { get; set; } = new List<FailedControlDto>();
    public IReadOnlyList<EvidenceGapDto> EvidenceGaps { get; set; } = new List<EvidenceGapDto>();
    public IReadOnlyList<ExceptionSummaryDto> ExceptionSummaries { get; set; } = new List<ExceptionSummaryDto>();
}