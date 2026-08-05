using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Compliance;

namespace VRTrackingApp.Web.Services.Compliance;

public interface IComplianceReportService
{
    Task<FrameworkSummaryDto> GetFrameworkSummaryAsync(string framework, CancellationToken ct = default);
    Task<IReadOnlyList<ControlCoverageDto>> GetControlCoverageAsync(string? framework = null, CancellationToken ct = default);
    Task<IReadOnlyList<FailedControlDto>> GetFailedControlsAsync(string? framework = null, CancellationToken ct = default);
    Task<IReadOnlyList<EvidenceGapDto>> GetEvidenceGapsAsync(string? framework = null, CancellationToken ct = default);
    Task<IReadOnlyList<ExceptionSummaryDto>> GetExceptionSummaryAsync(string? framework = null, CancellationToken ct = default);
}

public class ComplianceReportService : IComplianceReportService
{
    private readonly VRTrackingAppContext _db;

    public ComplianceReportService(VRTrackingAppContext db) => _db = db;

    public async Task<FrameworkSummaryDto> GetFrameworkSummaryAsync(string framework, CancellationToken ct = default)
    {
        var controls = await _db.ComplianceControls
            .Where(c => c.Framework == framework)
            .ToListAsync(ct);

        var links = await _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Where(fcl => fcl.Control.Framework == framework)
            .ToListAsync(ct);

        var totalControls = controls.Count;
        var mappedFindings = links.Count;
        var compliant = links.Count(l => l.Status == ComplianceStatus.Compliant);
        var nonCompliant = links.Count(l => l.Status == ComplianceStatus.NonCompliant);
        var inProgress = links.Count(l => l.Status == ComplianceStatus.InProgress);
        var notApplicable = links.Count(l => l.Status == ComplianceStatus.NotApplicable);
        var underReview = links.Count(l => l.Status == ComplianceStatus.UnderReview);
        var notMapped = totalControls > 0 ? totalControls - links.Select(l => l.ComplianceControlId).Distinct().Count() : 0;

        return new FrameworkSummaryDto
        {
            Framework = framework,
            TotalControls = totalControls,
            TotalFindings = mappedFindings,
            Compliant = compliant,
            NonCompliant = nonCompliant,
            InProgress = inProgress,
            NotApplicable = notApplicable,
            UnderReview = underReview,
            NotMapped = notMapped,
            CoveragePercent = totalControls > 0 ? Math.Round((double)(mappedFindings - nonCompliant) / totalControls * 100, 1) : 0
        };
    }

    public async Task<IReadOnlyList<ControlCoverageDto>> GetControlCoverageAsync(string? framework = null, CancellationToken ct = default)
    {
        var query = _db.ComplianceControls.AsQueryable();
        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(c => c.Framework == framework);

        var controls = await query.ToListAsync(ct);
        var result = new List<ControlCoverageDto>();

        foreach (var control in controls)
        {
            var linkCount = await _db.FindingComplianceLinks.CountAsync(l => l.ComplianceControlId == control.Id, ct);
            var compliantCount = await _db.FindingComplianceLinks.CountAsync(l => l.ComplianceControlId == control.Id && l.Status == ComplianceStatus.Compliant, ct);
            var nonCompliantCount = await _db.FindingComplianceLinks.CountAsync(l => l.ComplianceControlId == control.Id && l.Status == ComplianceStatus.NonCompliant, ct);

            result.Add(new ControlCoverageDto
            {
                ControlId = control.ControlId,
                ControlName = control.Name,
                Framework = control.Framework,
                ControlFamilyName = control.ControlFamilyNavigation?.Name,
                TotalFindings = linkCount,
                Compliant = compliantCount,
                NonCompliant = nonCompliantCount,
                CoveragePercent = linkCount > 0 ? Math.Round((double)compliantCount / linkCount * 100, 1) : 0
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<FailedControlDto>> GetFailedControlsAsync(string? framework = null, CancellationToken ct = default)
    {
        var query = _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Include(fcl => fcl.Finding)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(fcl => fcl.Control.Framework == framework);

        var failed = await query
            .Where(fcl => fcl.Status == ComplianceStatus.NonCompliant || fcl.Status == ComplianceStatus.UnderReview)
            .OrderByDescending(fcl => fcl.Finding.CvssV3BaseScore)
            .ToListAsync(ct);

        return failed.Select(fcl => new FailedControlDto
        {
            ControlId = fcl.Control.ControlId,
            ControlName = fcl.Control.Name,
            Framework = fcl.Control.Framework,
            FindingId = fcl.VulnerabilityFindingId,
            Cve = fcl.Finding.Cve,
            Severity = fcl.Finding.Severity,
            Status = fcl.Status,
            Rationale = fcl.Rationale,
            EvidenceRef = fcl.EvidenceRef,
            ReviewedBy = fcl.ReviewedBy,
            ReviewedAt = fcl.ReviewedAt
        }).ToList();
    }

    public async Task<IReadOnlyList<EvidenceGapDto>> GetEvidenceGapsAsync(string? framework = null, CancellationToken ct = default)
    {
        var query = _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Include(fcl => fcl.Finding)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(fcl => fcl.Control.Framework == framework);

        var links = await query
            .Where(fcl => fcl.Status == ComplianceStatus.Compliant && string.IsNullOrEmpty(fcl.EvidenceRef))
            .ToListAsync(ct);

        return links.Select(fcl => new EvidenceGapDto
        {
            ControlId = fcl.Control.ControlId,
            ControlName = fcl.Control.Name,
            Framework = fcl.Control.Framework,
            FindingId = fcl.VulnerabilityFindingId,
            Cve = fcl.Finding.Cve,
            Severity = fcl.Finding.Severity,
            Status = fcl.Status
        }).ToList();
    }

    public async Task<IReadOnlyList<ExceptionSummaryDto>> GetExceptionSummaryAsync(string? framework = null, CancellationToken ct = default)
    {
        var query = _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Include(fcl => fcl.Finding)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(fcl => fcl.Control.Framework == framework);

        var links = await query
            .Where(fcl => fcl.Status == ComplianceStatus.NonCompliant && !string.IsNullOrEmpty(fcl.Rationale))
            .ToListAsync(ct);

        return links.Select(fcl => new ExceptionSummaryDto
        {
            ControlId = fcl.Control.ControlId,
            ControlName = fcl.Control.Name,
            Framework = fcl.Control.Framework,
            FindingId = fcl.VulnerabilityFindingId,
            Cve = fcl.Finding.Cve,
            Severity = fcl.Finding.Severity,
            Rationale = fcl.Rationale,
            ReviewedBy = fcl.ReviewedBy,
            ReviewedAt = fcl.ReviewedAt
        }).ToList();
    }
}

public class FrameworkSummaryDto
{
    public string Framework { get; set; } = default!;
    public int TotalControls { get; set; }
    public int TotalFindings { get; set; }
    public int Compliant { get; set; }
    public int NonCompliant { get; set; }
    public int InProgress { get; set; }
    public int NotApplicable { get; set; }
    public int UnderReview { get; set; }
    public int NotMapped { get; set; }
    public double CoveragePercent { get; set; }
}

public class ControlCoverageDto
{
    public string ControlId { get; set; } = default!;
    public string ControlName { get; set; } = default!;
    public string Framework { get; set; } = default!;
    public string? ControlFamilyName { get; set; }
    public int TotalFindings { get; set; }
    public int Compliant { get; set; }
    public int NonCompliant { get; set; }
    public double CoveragePercent { get; set; }
}

public class FailedControlDto
{
    public string ControlId { get; set; } = default!;
    public string ControlName { get; set; } = default!;
    public string Framework { get; set; } = default!;
    public int FindingId { get; set; }
    public string? Cve { get; set; }
    public string? Severity { get; set; }
    public ComplianceStatus Status { get; set; }
    public string? Rationale { get; set; }
    public string? EvidenceRef { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class EvidenceGapDto
{
    public string ControlId { get; set; } = default!;
    public string ControlName { get; set; } = default!;
    public string Framework { get; set; } = default!;
    public int FindingId { get; set; }
    public string? Cve { get; set; }
    public string? Severity { get; set; }
    public ComplianceStatus Status { get; set; }
}

public class ExceptionSummaryDto
{
    public string ControlId { get; set; } = default!;
    public string ControlName { get; set; } = default!;
    public string Framework { get; set; } = default!;
    public int FindingId { get; set; }
    public string? Cve { get; set; }
    public string? Severity { get; set; }
    public string? Rationale { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}