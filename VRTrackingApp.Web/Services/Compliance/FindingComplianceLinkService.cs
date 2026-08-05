using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Compliance;

namespace VRTrackingApp.Web.Services.Compliance;

public interface IFindingComplianceLinkService
{
    Task<IReadOnlyList<FindingComplianceLink>> GetByFindingIdAsync(int findingId, CancellationToken ct = default);
    Task<FindingComplianceLink?> GetLinkAsync(int findingId, int controlId, CancellationToken ct = default);
    Task<FindingComplianceLink> CreateAsync(FindingComplianceLink link, CancellationToken ct = default);
    Task UpdateAsync(FindingComplianceLink link, CancellationToken ct = default);
    Task DeleteAsync(int findingId, int controlId, CancellationToken ct = default);
    Task<IReadOnlyList<FindingComplianceLink>> GetByControlIdAsync(int controlId, CancellationToken ct = default);
    Task<ComplianceSummaryDto> GetSummaryAsync(int findingId, CancellationToken ct = default);
}

public class FindingComplianceLinkService : IFindingComplianceLinkService
{
    private readonly VRTrackingAppContext _db;

    public FindingComplianceLinkService(VRTrackingAppContext db) => _db = db;

    public async Task<IReadOnlyList<FindingComplianceLink>> GetByFindingIdAsync(int findingId, CancellationToken ct = default)
    {
        return await _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Where(fcl => fcl.VulnerabilityFindingId == findingId)
            .OrderBy(fcl => fcl.Control.Framework)
            .ThenBy(fcl => fcl.Control.ControlId)
            .ToListAsync(ct);
    }

    public async Task<FindingComplianceLink?> GetLinkAsync(int findingId, int controlId, CancellationToken ct = default)
    {
        return await _db.FindingComplianceLinks
            .FirstOrDefaultAsync(fcl => fcl.VulnerabilityFindingId == findingId && fcl.ComplianceControlId == controlId, ct);
    }

    public async Task<FindingComplianceLink> CreateAsync(FindingComplianceLink link, CancellationToken ct = default)
    {
        link.CreatedAt = DateTime.UtcNow;
        _db.FindingComplianceLinks.Add(link);
        await _db.SaveChangesAsync(ct);
        return link;
    }

    public async Task UpdateAsync(FindingComplianceLink link, CancellationToken ct = default)
    {
        link.UpdatedAt = DateTime.UtcNow;
        _db.FindingComplianceLinks.Update(link);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int findingId, int controlId, CancellationToken ct = default)
    {
        var link = await GetLinkAsync(findingId, controlId, ct);
        if (link != null)
        {
            _db.FindingComplianceLinks.Remove(link);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<FindingComplianceLink>> GetByControlIdAsync(int controlId, CancellationToken ct = default)
    {
        return await _db.FindingComplianceLinks
            .Include(fcl => fcl.Finding)
            .Where(fcl => fcl.ComplianceControlId == controlId)
            .ToListAsync(ct);
    }

    public async Task<ComplianceSummaryDto> GetSummaryAsync(int findingId, CancellationToken ct = default)
    {
        var links = await _db.FindingComplianceLinks
            .Include(fcl => fcl.Control)
            .Where(fcl => fcl.VulnerabilityFindingId == findingId)
            .ToListAsync(ct);

        return new ComplianceSummaryDto
        {
            TotalControls = links.Count,
            Compliant = links.Count(l => l.Status == ComplianceStatus.Compliant),
            NonCompliant = links.Count(l => l.Status == ComplianceStatus.NonCompliant),
            InProgress = links.Count(l => l.Status == ComplianceStatus.InProgress),
            NotApplicable = links.Count(l => l.Status == ComplianceStatus.NotApplicable),
            UnderReview = links.Count(l => l.Status == ComplianceStatus.UnderReview),
            NotMapped = links.Count(l => l.Status == ComplianceStatus.NotMapped),
            Links = links
        };
    }
}

public class ComplianceSummaryDto
{
    public int TotalControls { get; set; }
    public int Compliant { get; set; }
    public int NonCompliant { get; set; }
    public int InProgress { get; set; }
    public int NotApplicable { get; set; }
    public int UnderReview { get; set; }
    public int NotMapped { get; set; }
    public IReadOnlyList<FindingComplianceLink> Links { get; set; } = new List<FindingComplianceLink>();
}