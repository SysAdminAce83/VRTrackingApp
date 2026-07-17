using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

public class HostsController : Controller
{
    private readonly VRTrackingAppContext _db;
    public HostsController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        ViewData["Title"] = "Hosts";
        var hosts = _db.AssetHosts
            .Include(h => h.ScanUpload)
            .Include(h => h.Instances).ThenInclude(i => i.VulnerabilityFinding)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            hosts = hosts.Where(h => h.HostName!.Contains(q) || h.IpAddress.Contains(q));

        var list = await hosts.OrderBy(h => h.HostName).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id, string? status)
    {
        var host = await _db.AssetHosts
            .Include(h => h.ScanUpload)
            .Include(h => h.Instances).ThenInclude(i => i.VulnerabilityFinding)
            .Include(h => h.Instances).ThenInclude(i => i.Owner)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (host == null) return NotFound();

        ViewData["Title"] = host.HostName ?? "Host";
        var inst = host.Instances.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            inst = inst.Where(i => i.Status == status);

        var bySev = host.Instances.GroupBy(i => i.VulnerabilityFinding!.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        ViewBag.Status = status ?? "All";
        ViewBag.BySev = bySev;
        ViewBag.Filtered = inst.OrderByDescending(i => i.VulnerabilityFinding!.Severity).ToList();
        return View(host);
    }
}
