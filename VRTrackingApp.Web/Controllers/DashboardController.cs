using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

public class DashboardController : Controller
{
    private readonly VRTrackingAppContext _db;
    public DashboardController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var instances = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .ToListAsync();

        var hosts = await _db.AssetHosts.CountAsync();
        var findings = await _db.VulnerabilityFindings.CountAsync();
        var open = instances.Count(i => i.Status == "Open");
        var fixedN = instances.Count(i => i.Status == "Fixed");
        var exc = instances.Count(i => i.Status == "Exception");

        var severityCounts = new[] { "Critical", "High", "Medium", "Low", "Info" }
            .ToDictionary(s => s, s => instances.Count(i => i.VulnerabilityFinding!.Severity == s));

        var critHigh = severityCounts["Critical"] + severityCounts["High"];

        // Remediation trend by scan cycle
        var cycles = instances
            .Where(i => i.AssetHost?.ScanUpload?.ScanCycleLabel != null)
            .GroupBy(i => i.AssetHost!.ScanUpload!.ScanCycleLabel!)
            .OrderBy(g => g.First().AssetHost!.ScanUpload!.ScanDate)
            .Select(g => new CycleStat(
                g.Key,
                g.Count(i => i.Status == "Open"),
                g.Count(i => i.Status == "Fixed"),
                g.Count(i => i.Status == "Exception")))
            .ToList();

        // Severity distribution (unique findings)
        var findingSev = await _db.VulnerabilityFindings
            .GroupBy(f => f.Severity)
            .Select(g => new { Sev = g.Key, Count = g.Count() })
            .ToListAsync();
        var sevDist = new[] { "Critical", "High", "Medium", "Low", "Info" }
            .ToDictionary(s => s, s => findingSev.FirstOrDefault(x => x.Sev == s)?.Count ?? 0);

        var recentScans = await _db.ScanUploads
            .Include(s => s.AssetHosts)
            .Include(s => s.UploadedBy)
            .OrderByDescending(s => s.UploadedAt)
            .Take(6).ToListAsync();

        var overdue = instances
            .Where(i => i.Status != "Fixed" && i.DueDate < DateTime.UtcNow)
            .OrderBy(i => i.DueDate)
            .Take(8).ToList();

        var expiring = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Where(e => e.ExpiresAt < DateTime.UtcNow.AddDays(30))
            .OrderBy(e => e.ExpiresAt)
            .Take(8).ToListAsync();

        ViewData["Title"] = "Dashboard";
        ViewBag.Kpis = new DasKpis(hosts, findings, open, fixedN, exc, critHigh, severityCounts["Critical"], severityCounts["High"]);
        ViewBag.Cycles = cycles;
        ViewBag.SevDist = sevDist;
        ViewBag.RecentScans = recentScans;
        ViewBag.Overdue = overdue;
        ViewBag.Expiring = expiring;
        return View();
    }

    public record CycleStat(string Label, int Open, int Fixed, int Exception);
    public record DasKpis(int Hosts, int Findings, int Open, int Fixed, int Exceptions, int CritHigh,
        int Critical, int High);
}
