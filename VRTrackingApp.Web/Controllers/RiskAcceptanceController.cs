using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Compliance;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin,Analyst")]
public class RiskAcceptanceController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly IComplianceControlService _controlService;
    private readonly IFindingComplianceLinkService _linkService;

    public RiskAcceptanceController(
        VRTrackingAppContext db,
        IComplianceControlService controlService,
        IFindingComplianceLinkService linkService)
    {
        _db = db;
        _controlService = controlService;
        _linkService = linkService;
    }

    public async Task<IActionResult> Index(string? framework, string? status, int page = 1)
    {
        ViewData["Title"] = "Risk Acceptances";
        ViewBag.Framework = framework;
        ViewBag.Status = status;

        var query = _db.RiskAcceptances
            .Include(ra => ra.Finding)
            .Include(ra => ra.Control)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(framework))
            query = query.Where(ra => ra.Control.Framework == framework);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(ra => ra.Status.ToString() == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(ra => ra.AcceptedAt)
            .Skip((page - 1) * 20)
            .Take(20)
            .ToListAsync();

        ViewBag.Total = total;
        ViewBag.Page = page;

        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var acceptance = await _db.RiskAcceptances
            .Include(ra => ra.Finding)
            .Include(ra => ra.Control)
            .FirstOrDefaultAsync(ra => ra.Id == id);

        if (acceptance == null) return NotFound();
        return View(acceptance);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int findingId, int controlId, string justification, DateTime? expiryDate)
    {
        var finding = await _db.VulnerabilityFindings.FindAsync(findingId);
        var control = await _db.ComplianceControls.FindAsync(controlId);
        if (finding == null || control == null) return NotFound();

        var acceptance = new RiskAcceptance
        {
            VulnerabilityFindingId = findingId,
            ComplianceControlId = controlId,
            Justification = justification,
            AcceptedBy = User.Identity?.Name ?? "Unknown",
            AcceptedAt = DateTime.UtcNow,
            ExpiryDate = expiryDate,
            Status = RiskAcceptanceStatus.Active
        };

        _db.RiskAcceptances.Add(acceptance);

        var link = await _linkService.GetLinkAsync(findingId, controlId);
        if (link != null)
        {
            link.Status = ComplianceStatus.UnderReview;
            link.Rationale = justification;
            await _linkService.UpdateAsync(link);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Details", "Vulnerabilities", new { id = findingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id)
    {
        var acceptance = await _db.RiskAcceptances.FindAsync(id);
        if (acceptance == null) return NotFound();

        acceptance.Status = RiskAcceptanceStatus.Revoked;
        acceptance.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}