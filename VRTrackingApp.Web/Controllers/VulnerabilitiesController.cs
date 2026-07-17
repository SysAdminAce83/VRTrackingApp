using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Remediation;

namespace VRTrackingApp.Web.Controllers;

public class VulnerabilitiesController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly RemediationEngine _remediation;
    private readonly IRemediationQueue _queue;

    public VulnerabilitiesController(VRTrackingAppContext db, RemediationEngine remediation, IRemediationQueue queue)
    {
        _db = db;
        _remediation = remediation;
        _queue = queue;
    }

    public async Task<IActionResult> Index(string? q, string? severity, string? status)
    {
        ViewData["Title"] = "Vulnerabilities";
        ViewBag.Q = q; ViewBag.Severity = severity; ViewBag.Status = status;

        var inst = _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost)
            .Include(i => i.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            inst = inst.Where(i => i.VulnerabilityFinding!.PluginName.Contains(q)
                || i.VulnerabilityFinding!.Cve != null && i.VulnerabilityFinding!.Cve.Contains(q)
                || i.AssetHost!.HostName!.Contains(q)
                || i.AssetHost.IpAddress.Contains(q));
        if (!string.IsNullOrWhiteSpace(severity) && severity != "All")
            inst = inst.Where(i => i.VulnerabilityFinding!.Severity == severity);
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            inst = inst.Where(i => i.Status == status);

        var list = await inst
            .OrderByDescending(i => i.VulnerabilityFinding!.Severity)
            .ThenBy(i => i.AssetHost!.HostName)
            .Take(500).ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var inst = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .Include(i => i.AssetHost).ThenInclude(h => h!.Asset)
            .Include(i => i.Owner)
            .Include(i => i.RemediationActions).ThenInclude(r => r.AssignedTo)
            .Include(i => i.ExceptionRecord)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inst == null) return NotFound();

        ViewData["Title"] = $"Plugin {inst.VulnerabilityFinding?.PluginId}";
        ViewBag.Owners = await _db.UserAccounts.Where(u => u.IsActive).ToListAsync();
        ViewBag.LatestJob = await _db.RemediationJobs
            .Where(j => j.VulnerabilityInstanceId == id)
            .OrderByDescending(j => j.Id)
            .FirstOrDefaultAsync();
        return View(inst);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRemediation(int id, string status, int? ownerId,
        DateTime? dueDate, string? comments)
    {
        var inst = await _db.VulnerabilityInstances
            .Include(i => i.RemediationActions)
            .Include(i => i.ExceptionRecord)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inst == null) return NotFound();

        // If a vulnerability that had an exception is now fixed/reopened, drop the exception.
        if (status != "Exception" && inst.ExceptionRecord != null)
            _db.ExceptionRecords.Remove(inst.ExceptionRecord);

        inst.Status = status == "Exception" ? "Under Review" : status;
        inst.OwnerUserId = ownerId;
        inst.DueDate = dueDate;

        inst.RemediationActions.Add(new RemediationAction
        {
            Action = status,
            Status = status,
            AssignedToUserId = ownerId,
            DueDate = dueDate,
            Comments = comments,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    // ---- Automated remediation -------------------------------------------------

    /// <summary>Queue a read-only check (is the patch installed / available / reboot pending).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Check(int id)
    {
        var jobId = await _remediation.CreateJobAsync(id, RemediationJobTypes.Check, CurrentUserId());
        await _queue.EnqueueAsync(jobId);
        return Json(new { jobId });
    }

    /// <summary>Queue an install attempt. Critical assets are blocked by the engine.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Remediation Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remediate(int id)
    {
        var jobId = await _remediation.CreateJobAsync(id, RemediationJobTypes.Install, CurrentUserId());
        await _queue.EnqueueAsync(jobId);
        return Json(new { jobId });
    }

    /// <summary>Polled by the Details page to show live job progress.</summary>
    [HttpGet]
    public async Task<IActionResult> JobStatus(int jobId)
    {
        var job = await _db.RemediationJobs.FindAsync(jobId);
        if (job == null) return NotFound();
        return Json(new
        {
            job.Id,
            job.JobType,
            job.State,
            job.TargetHost,
            job.PatchId,
            job.ResultSummary,
            job.Log,
            done = job.State is not (RemediationJobStates.Queued or RemediationJobStates.Running)
        });
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) && uid >= 0 ? uid : null;
}
