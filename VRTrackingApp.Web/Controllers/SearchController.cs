using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

public class SearchController : Controller
{
    private readonly VRTrackingAppContext _db;
    public SearchController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index(string? q)
    {
        ViewData["Title"] = "Search";
        if (string.IsNullOrWhiteSpace(q))
            return View(new SearchResult(new List<AssetHost>(), new List<VulnerabilityInstance>()));

        var hosts = await _db.AssetHosts
            .Include(h => h.Instances)
            .Where(h => h.HostName!.Contains(q) || h.IpAddress.Contains(q))
            .Take(20).ToListAsync();

        var inst = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost)
            .Where(i => i.VulnerabilityFinding!.PluginName.Contains(q)
                || (i.VulnerabilityFinding!.Cve != null && i.VulnerabilityFinding!.Cve.Contains(q)))
            .Take(50).ToListAsync();

        return View(new SearchResult(hosts, inst));
    }

    public record SearchResult(List<AssetHost> Hosts, List<VulnerabilityInstance> Instances);
}
