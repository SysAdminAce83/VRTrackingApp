using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

public class ScansController : Controller
{
    private readonly VRTrackingAppContext _db;
    public ScansController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Scans";
        var scans = await _db.ScanUploads
            .Include(s => s.AssetHosts)
            .Include(s => s.UploadedBy)
            .OrderByDescending(s => s.ScanDate ?? s.UploadedAt)
            .ToListAsync();

        var rows = scans.Select(s =>
        {
            var hostIds = s.AssetHosts.Select(h => h.Id).ToList();
            var findingCount = _db.VulnerabilityInstances.Count(i => hostIds.Contains(i.AssetHostId));
            return new ScanRow(s, s.AssetHosts.Count, findingCount);
        }).ToList();

        return View(rows);
    }

    public record ScanRow(ScanUpload Scan, int HostCount, int FindingCount);

    public async Task<IActionResult> Details(int id, int? hostId, string? severity, string? status, string? q)
    {
        var scan = await _db.ScanUploads
            .Include(s => s.AssetHosts).ThenInclude(h => h.Instances).ThenInclude(i => i.VulnerabilityFinding)
            .Include(s => s.UploadedBy)
            .Include(s => s.AuditTrail).ThenInclude(t => t.PerformedBy)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (scan == null) return NotFound();

        ViewData["Title"] = scan.ScanCycleLabel ?? "Scan";
        var hostIds = scan.AssetHosts.Select(h => h.Id).ToList();
        var instancesQ = _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost)
            .Where(i => hostIds.Contains(i.AssetHostId));
        if (!string.IsNullOrEmpty(severity)) instancesQ = instancesQ.Where(i => i.VulnerabilityFinding!.Severity == severity);
        if (!string.IsNullOrEmpty(status)) instancesQ = instancesQ.Where(i => i.Status == status);
        if (!string.IsNullOrEmpty(q)) instancesQ = instancesQ.Where(i => i.VulnerabilityFinding!.PluginName.Contains(q) || i.VulnerabilityFinding.Cve!.Contains(q) || i.AssetHost!.HostName.Contains(q));
        var allInstances = await instancesQ.ToListAsync();

        var selected = hostId.HasValue
            ? scan.AssetHosts.FirstOrDefault(h => h.Id == hostId.Value)
            : scan.AssetHosts.FirstOrDefault();

        ViewBag.Scan = scan;
        ViewBag.AllInstances = allInstances;
        ViewBag.SelectedHost = selected;
        ViewBag.FilterSeverity = severity;
        ViewBag.FilterStatus = status;
        ViewBag.FilterQ = q;
        var total = scan.AssetHosts.SelectMany(h => h.Instances).Count();
        var fixedCnt = scan.AssetHosts.SelectMany(h => h.Instances).Count(i => i.Status == "Fixed");
        ViewBag.RemediationProgress = total == 0 ? 0 : (int)Math.Round(100.0 * fixedCnt / total);
        return View();
    }
}
