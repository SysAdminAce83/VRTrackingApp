using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin,SecurityChampion")]
public class AuditController : Controller
{
    private readonly VRTrackingAppContext _db;
    public AuditController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index(string? category, string? act)
    {
        ViewData["Title"] = "Audit Log";
        var q = _db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category) && category != "All") q = q.Where(a => a.Category == category);
        if (!string.IsNullOrWhiteSpace(act) && act != "All") q = q.Where(a => a.Action == act);

        var items = await q.OrderByDescending(a => a.PerformedAt).Take(500).ToListAsync();
        ViewBag.Category = category ?? "All";
        ViewBag.Action = act ?? "All";
        ViewBag.Categories = new[] { "All", "User", "Exception", "Role" };
        ViewBag.Actions = new[] { "All", "Created", "Edited", "Deleted", "Approved", "Rejected", "Requested modification", "Requested deletion" };
        return View(items);
    }
}
