using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Html;
using VRTrackingApp.Web.Services;
using VRTrackingApp.Web.Services.Exceptions;
using VRTrackingApp.Web.Services.Notifications;

namespace VRTrackingApp.Web.Controllers;

public class ExceptionsController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly ExceptionWorkflowService _wf;
    private readonly ExceptionRoutingService _routing;
    private readonly AuditLogService _audit;
    private readonly INotificationService _notify;
    private readonly IWebHostEnvironment _env;

    public ExceptionsController(VRTrackingAppContext db, ExceptionWorkflowService wf,
        ExceptionRoutingService routing, AuditLogService audit, INotificationService notify, IWebHostEnvironment env)
    {
        _db = db;
        _wf = wf;
        _routing = routing;
        _audit = audit;
        _notify = notify;
        _env = env;
    }

    // ----------------------------------------------------------------- Index
    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? severity, string? risk)
    {
        ViewData["Title"] = "Exceptions";
        var q = _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost)
            .Include(e => e.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            q = q.Where(e => e.Status.ToString() == status);
        if (!string.IsNullOrWhiteSpace(severity) && severity != "All")
            q = q.Where(e => e.VulnerabilityInstance!.VulnerabilityFinding!.Severity == severity);
        if (!string.IsNullOrWhiteSpace(risk) && risk != "All")
            q = q.Where(e => e.OverallRisk.ToString() == risk);

        var all = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .ToListAsync();
        var now = DateTime.UtcNow;
        ViewData["Kpi"] = new Dictionary<string, int>
        {
            ["Active"] = all.Count(e => e.Status == ExceptionStatus.ActiveException),
            ["Expired"] = all.Count(e => e.Status == ExceptionStatus.Expired),
            ["Expiring30"] = all.Count(e => e.Status == ExceptionStatus.ActiveException && e.ExpiryDate.HasValue && e.ExpiryDate < now.AddDays(30)),
            ["Critical"] = all.Count(e => e.VulnerabilityInstance != null && e.VulnerabilityInstance.VulnerabilityFinding != null && e.VulnerabilityInstance.VulnerabilityFinding.Severity == "Critical" && e.Status == ExceptionStatus.ActiveException),
            ["Pending"] = all.Count(e => e.Status is ExceptionStatus.PendingTechnicalApproval or ExceptionStatus.PendingManagerApproval or ExceptionStatus.PendingSecurityApproval),
            ["Rejected"] = all.Count(e => e.Status == ExceptionStatus.Rejected),
            ["Closed"] = all.Count(e => e.Status == ExceptionStatus.Closed),
        };

        var list = await q.OrderByDescending(e => e.CreatedAt).ThenBy(e => e.VulnerabilityInstance!.AssetHost!.HostName).ToListAsync();
        ViewBag.Status = status; ViewBag.Severity = severity; ViewBag.Risk = risk;
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_ExceptionsTable", list);
        return View(list);
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ex = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost).ThenInclude(h => h.Asset)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .Include(e => e.Owner)
            .Include(e => e.Mitigations)
            .Include(e => e.Evidence)
            .Include(e => e.SecurityControls)
            .Include(e => e.ApprovalSteps).ThenInclude(s => s.DecisionBy)
            .Include(e => e.Reviews)
            .Include(e => e.Comments).ThenInclude(c => c.User)
            .Include(e => e.VendorResponses)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null) return NotFound();

        ViewData["Title"] = $"Exception #{ex.Id}";
        ViewBag.CanAct = _wf.CanActOnCurrent(ex, CurrentRoles());
        ViewBag.IsOwner = ex.OwnerUserId == CurrentUserId();
        return View(ex);
    }

    // ----------------------------------------------------------------- Request
    [HttpGet]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    public async Task<IActionResult> RequestForm(int instanceId)
    {
        var inst = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.Asset)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == instanceId);
        if (inst == null) return NotFound();

        if (inst.ExceptionRecord != null)
            return RedirectToAction("Details", new { id = inst.ExceptionRecord.Id });

        ViewData["Title"] = "Request Exception";
        ViewBag.Stage1Role = _routing.ResolveStage1Role(inst);
        ViewBag.Owners = await _db.UserAccounts.Where(u => u.IsActive).ToListAsync();
        return View(inst);
    }

    // ----------------------------------------------------------------- Create
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int instanceId, string reason, NonFixableReason nonFixableReason,
        string? otherReasonText, string? technicalJustification, string? downtimeConstraint, string? businessImpact,
        string? costImpact, string? productionImpact, string? customerImpact, string? complianceImpact,
        Likelihood? likelihood, ImpactLevel? impact, bool affectsConfidentiality, bool affectsIntegrity, bool affectsAvailability,
        Exploitability? exploitability, InternetExposure? internetExposure, int? reviewFrequencyDays, DateTime? expiryDate,
        string? ownerUserId, string[]? mitigations, string[]? mitigationStatus, string[]? controls,
        List<IFormFile>? evidence, string? evidenceType, string? comment)
    {
        var inst = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost)
            .Include(i => i.ExceptionRecord)
            .FirstOrDefaultAsync(i => i.Id == instanceId);
        if (inst == null) return NotFound();
        if (inst.ExceptionRecord != null) return RedirectToAction("Details", new { id = inst.ExceptionRecord.Id });

        if (string.IsNullOrWhiteSpace(reason))
            ModelState.AddModelError(nameof(reason), "A justification / reason is required.");

        var ex = new ExceptionRecord
        {
            VulnerabilityInstance = inst,
            Reason = reason,
            OwnerUserId = int.TryParse(ownerUserId, out var oid) ? oid : (int?)null,
            NonFixableReason = nonFixableReason,
            OtherReasonText = nonFixableReason == NonFixableReason.Other ? otherReasonText : null,
            TechnicalJustification = technicalJustification,
            DowntimeConstraint = downtimeConstraint,
            BusinessImpact = businessImpact,
            CostImpact = costImpact,
            ProductionImpact = productionImpact,
            CustomerImpact = customerImpact,
            ComplianceImpact = complianceImpact,
            Likelihood = likelihood,
            Impact = impact,
            OverallRisk = RiskMatrixService.CalculateOrNull(likelihood, impact),
            AffectsConfidentiality = affectsConfidentiality,
            AffectsIntegrity = affectsIntegrity,
            AffectsAvailability = affectsAvailability,
            Exploitability = exploitability,
            InternetExposure = internetExposure,
            ReviewFrequencyDays = reviewFrequencyDays ?? 90,
            ExpiryDate = expiryDate,
            CreatedAt = DateTime.UtcNow,
            Status = ExceptionStatus.ExceptionRequested
        };

        if (mitigations != null)
            for (var i = 0; i < mitigations.Length; i++)
                if (!string.IsNullOrWhiteSpace(mitigations[i]))
                    ex.Mitigations.Add(new ExceptionMitigation
                    {
                        Description = mitigations[i],
                        Status = i < mitigationStatus?.Length && Enum.TryParse<MitigationStatus>(mitigationStatus[i], out var ms) ? ms : MitigationStatus.Planned
                    });

        if (controls != null)
            foreach (var c in controls.Where(c => !string.IsNullOrWhiteSpace(c)))
                ex.SecurityControls.Add(new ExceptionSecurityControl { ControlName = c });

        var stage1 = _routing.ResolveStage1Role(inst);
        _wf.StartApproval(ex, stage1);
        _db.ExceptionRecords.Add(ex);

        if (evidence != null)
            foreach (var file in evidence.Where(fl => fl != null && fl.Length > 0))
                ex.Evidence.Add(await SaveEvidenceAsync(file, evidenceType, CurrentUserId()));

        if (!ModelState.IsValid)
        {
            await _db.Entry(ex).Reference(e => e.VulnerabilityInstance).LoadAsync();
            ViewData["Title"] = "Request Exception";
            ViewBag.Stage1Role = stage1;
            ViewBag.Owners = await _db.UserAccounts.Where(u => u.IsActive).ToListAsync();
            return View("Request", inst);
        }

        await _db.SaveChangesAsync();

        // Mark instance as requested so it no longer reads "Open" in the same way.
        if (inst.Status == "Open") inst.Status = "Under Review";
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(comment))
            ex.Comments.Add(new ExceptionComment { UserId = CurrentUserId(), AuthorDisplayName = User.Identity?.Name, Body = comment, CreatedAt = DateTime.UtcNow });

        await _audit.LogAsync("Exception", "Requested", $"exception #{ex.Id} ({(inst.VulnerabilityFinding?.PluginName ?? "vuln")})",
            $"Stage-1 approver: {ExceptionRoutingService.RoleLabel(stage1)}. Risk: {ex.OverallRisk}.", CurrentUserId());

        return RedirectToAction("Details", new { id = ex.Id });
    }

    // ----------------------------------------------------------------- Approval (wired in P2)
    [HttpPost]
    [Authorize(Roles = "Admin,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(int id, string decision, string? comment)
    {
        var ex = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.Owner)
            .Include(e => e.ApprovalSteps)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null) return NotFound();

        if (!_wf.CanActOnCurrent(ex, CurrentRoles()))
            return Forbid();

        var parsed = decision switch
        {
            "approve" => ApprovalDecision.Approved,
            "reject" => ApprovalDecision.Rejected,
            "info" => ApprovalDecision.NeedMoreInfo,
            _ => ApprovalDecision.Approved
        };
        if (parsed != ApprovalDecision.Approved && string.IsNullOrWhiteSpace(comment))
            ModelState.AddModelError(nameof(comment), "A comment is required for this decision.");

        var result = _wf.RecordDecision(ex, parsed, CurrentUserId()!.Value, comment ?? "");
        await _db.SaveChangesAsync();

        if (result == WorkflowResult.NeedMoreInfo && ex.Owner != null)
        {
            await _notify.NotifyUserAsync(ex.Owner, NotificationType.NeedMoreInfo, ex.Id,
                "More information requested", comment ?? "The approver requested more information before continuing.", default);
        }

        var labels = new Dictionary<WorkflowResult, (string Action, string Detail)>
        {
            [WorkflowResult.Advanced] = ("Approved stage", $"Advanced to {ExceptionStatusLabels.For(ex.Status)}."),
            [WorkflowResult.Approved] = ("Approved", "Exception is now active."),
            [WorkflowResult.Rejected] = ("Rejected", comment),
            [WorkflowResult.NeedMoreInfo] = ("Requested more info", comment),
        };
        if (labels.TryGetValue(result, out var info))
            await _audit.LogAsync("Exception", info.Action, $"exception #{ex.Id}", info.Detail, CurrentUserId());

        return RedirectToAction("Details", new { id = ex.Id });
    }

    // ----------------------------------------------------------------- Resubmit (after "need more info")
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit(int id, string? comment)
    {
        var ex = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.Owner)
            .Include(e => e.ApprovalSteps)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (ex == null) return NotFound();
        if (ex.Status != ExceptionStatus.NeedMoreInfo) return RedirectToAction("Details", new { id });

        var isOwner = ex.OwnerUserId == CurrentUserId();
        if (!isOwner && !User.IsInRole(AppRoles.Admin)) return Forbid();

        _wf.Resubmit(ex);
        if (!string.IsNullOrWhiteSpace(comment))
            ex.Comments.Add(new ExceptionComment
            {
                UserId = CurrentUserId(),
                AuthorDisplayName = User.Identity?.Name,
                Body = $"Resubmitted with additional information: {comment}",
                CreatedAt = DateTime.UtcNow
            });
        await _db.SaveChangesAsync();

        // Let the current-stage approver know it's back in their queue.
        if (ex.CurrentApprovalStage != null)
        {
            var role = ex.ApprovalSteps
                .FirstOrDefault(s => s.Stage == ex.CurrentApprovalStage)?.RequiredRole
                ?? ex.Stage1Role;
            if (!string.IsNullOrWhiteSpace(role))
                await _notify.NotifyRoleAsync(NotificationType.NeedMoreInfo, new[] { role }, ex.Id,
                    "Exception resubmitted",
                    $"Exception #{ex.Id} was resubmitted with more information and is awaiting your review again.",
                    default);
        }

        await _audit.LogAsync("Exception", "Resubmitted", $"exception #{ex.Id}", comment, CurrentUserId());
        return RedirectToAction("Details", new { id });
    }

    // ----------------------------------------------------------------- Comment / Evidence
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string body)
    {
        var ex = await _db.ExceptionRecords.FindAsync(id);
        if (ex == null) return NotFound();
        if (string.IsNullOrWhiteSpace(body)) return RedirectToAction("Details", new { id });

        ex.Comments.Add(new ExceptionComment
        {
            UserId = CurrentUserId(),
            AuthorDisplayName = User.Identity?.Name,
            Body = body,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadEvidence(int id)
    {
        var ev = await _db.ExceptionEvidence.FindAsync(id);
        if (ev == null) return NotFound();
        var path = Path.Combine(_env.WebRootPath, "uploads", "evidence", ev.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, "application/octet-stream", ev.OriginalFileName);
    }

    // ----------------------------------------------------------------- Evidence management (P6)
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEvidence(int id, List<IFormFile>? evidence, string[]? evidenceType)
    {
        var ex = await _db.ExceptionRecords.FindAsync(id);
        if (ex == null) return NotFound();
        if (evidence != null)
        {
            for (var i = 0; i < evidence.Count; i++)
            {
                var file = evidence[i];
                if (file == null || file.Length == 0) continue;
                var type = i < evidenceType?.Length ? evidenceType[i] : null;
                ex.Evidence.Add(await SaveEvidenceAsync(file, type, CurrentUserId()));
            }
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvidence(int id, int evidenceId)
    {
        var ev = await _db.ExceptionEvidence.FirstOrDefaultAsync(e => e.Id == evidenceId && e.ExceptionRecordId == id);
        if (ev == null) return NotFound();
        var path = Path.Combine(_env.WebRootPath, "uploads", "evidence", ev.StoredFileName);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        _db.ExceptionEvidence.Remove(ev);
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    // ----------------------------------------------------------------- Mitigation management (P6)
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMitigation(int id, string description, MitigationStatus status)
    {
        var ex = await _db.ExceptionRecords.FindAsync(id);
        if (ex == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(description))
        {
            ex.Mitigations.Add(new ExceptionMitigation
            {
                Description = description,
                Status = Enum.IsDefined(status) ? status : MitigationStatus.Planned,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMitigation(int id, int mitigationId, string description, MitigationStatus status)
    {
        var m = await _db.ExceptionMitigations.FirstOrDefaultAsync(x => x.Id == mitigationId && x.ExceptionRecordId == id);
        if (m == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(description))
            m.Description = description;
        m.Status = Enum.IsDefined(status) ? status : m.Status;
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Remediation Owner,SecurityChampion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMitigation(int id, int mitigationId)
    {
        var m = await _db.ExceptionMitigations.FirstOrDefaultAsync(x => x.Id == mitigationId && x.ExceptionRecordId == id);
        if (m == null) return NotFound();
        _db.ExceptionMitigations.Remove(m);
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    // ----------------------------------------------------------------- Vendor response (P6)
    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion,InfrastructureManager,NetworkManager,RiskCommittee,CISO")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVendorResponse(int id, string vendor, string responseText, DateTime? patchEtaDate)
    {
        var ex = await _db.ExceptionRecords.FindAsync(id);
        if (ex == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(vendor) && !string.IsNullOrWhiteSpace(responseText))
        {
            ex.VendorResponses.Add(new VendorResponse
            {
                Vendor = vendor,
                ResponseText = responseText,
                PatchEtaDate = patchEtaDate
            });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Analyst,Remediation Owner,SecurityChampion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVendorResponse(int id, int vendorResponseId)
    {
        var v = await _db.VendorResponses.FirstOrDefaultAsync(x => x.Id == vendorResponseId && x.ExceptionRecordId == id);
        if (v == null) return NotFound();
        _db.VendorResponses.Remove(v);
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    // ----------------------------------------------------------------- Dashboard (P3)
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "Exception Dashboard";
        var now = DateTime.UtcNow;
        var exs = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost).ThenInclude(h => h.Asset)
            .Include(e => e.Owner)
            .Include(e => e.Evidence)
            .Include(e => e.Mitigations)
            .ToListAsync();

        bool IsActive(ExceptionStatus s) => s is ExceptionStatus.ActiveException or ExceptionStatus.Renewed;
        bool IsPending(ExceptionStatus s) => s is ExceptionStatus.PendingTechnicalApproval
            or ExceptionStatus.PendingManagerApproval or ExceptionStatus.PendingSecurityApproval;

        ViewBag.Kpis = new ExceptionKpis(
            exs.Count,
            exs.Count(e => IsActive(e.Status)),
            exs.Count(e => IsPending(e.Status)),
            exs.Count(e => e.Status == ExceptionStatus.ActiveException && e.VulnerabilityInstance?.VulnerabilityFinding?.Severity == "Critical"),
            exs.Count(e => e.Status == ExceptionStatus.ActiveException && e.ExpiryDate.HasValue && e.ExpiryDate < now.AddDays(30)),
            exs.Count(e => e.Status == ExceptionStatus.Expired),
            exs.Count(e => e.Status == ExceptionStatus.Rejected),
            exs.Count(e => e.Status == ExceptionStatus.ReviewDue),
            exs.Count(e => e.Status == ExceptionStatus.Closed),
            exs.Count(e => e.Status == ExceptionStatus.NeedMoreInfo),
            exs.Count(e => e.Status == ExceptionStatus.ActiveException && e.Evidence.Count == 0),
            exs.Count(e => e.Status == ExceptionStatus.ActiveException && e.Mitigations.Any(m => m.Status == MitigationStatus.Pending))
        );

        // Trend by status (stacked, per created-month).
        ViewBag.Trend = exs
            .Where(e => e.CreatedAt != default)
            .GroupBy(e => e.CreatedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new TrendPoint(
                g.Key,
                g.Count(e => IsActive(e.Status)),
                g.Count(e => IsPending(e.Status) || e.Status is ExceptionStatus.ExceptionRequested or ExceptionStatus.NeedMoreInfo),
                g.Count(e => e.Status == ExceptionStatus.Rejected),
                g.Count(e => e.Status == ExceptionStatus.Expired),
                g.Count(e => e.Status == ExceptionStatus.Closed)))
            .ToList();

        // By reason, owner, business unit.
        ViewBag.ByReason = exs
            .Where(e => e.NonFixableReason.HasValue)
            .GroupBy(e => e.NonFixableReason!.Value)
            .Select(g => new NameCount(g.Key.Humanize(), g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
        ViewBag.ByOwner = exs
            .GroupBy(e => e.Owner?.DisplayName ?? "Unassigned")
            .Select(g => new NameCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();
        ViewBag.ByBu = exs
            .GroupBy(e => e.VulnerabilityInstance?.AssetHost?.Asset?.BusUnit
                        ?? e.VulnerabilityInstance?.AssetHost?.Asset?.Location
                        ?? "Unknown")
            .Select(g => new NameCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        return View();
    }

    // ----------------------------------------------------------------- Export
    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var exs = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.AssetHost)
            .Include(e => e.Owner)
            .Include(e => e.Evidence)
            .Include(e => e.Mitigations)
            .OrderBy(e => e.Id)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,Host,Finding,CVE,Severity,Risk,Status,Owner,Reason,ExpiryDate,NextReviewDate,CreatedAt,EvidenceCount,MitigationCount,ApprovalStage");
        foreach (var e in exs)
        {
            var f = e.VulnerabilityInstance?.VulnerabilityFinding;
            var host = e.VulnerabilityInstance?.AssetHost;
            sb.AppendLine(string.Join(",",
                Csv(e.Id.ToString()),
                Csv(host?.HostName),
                Csv(f?.PluginName),
                Csv(f?.Cve),
                Csv(f?.Severity),
                Csv(e.OverallRisk?.ToString()),
                Csv(ExceptionStatusLabels.For(e.Status)),
                Csv(e.Owner?.DisplayName),
                Csv(e.NonFixableReason?.Humanize()),
                Csv(e.ExpiryDate?.ToString("yyyy-MM-dd")),
                Csv(e.NextReviewDate?.ToString("yyyy-MM-dd")),
                Csv(e.CreatedAt.ToString("yyyy-MM-dd")),
                e.Evidence.Count.ToString(),
                e.Mitigations.Count.ToString(),
                Csv(e.CurrentApprovalStage?.ToString())
            ));
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"exception_register_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    private static string Csv(string? v) => $"\"{(v ?? "").Replace("\"", "\"\"")}\"";

    public record ExceptionKpis(int Total, int Active, int Pending, int Critical, int Expiring30,
        int Expired, int Rejected, int ReviewDue, int Closed, int NeedInfo, int MissingEvidence, int OverdueMitigation);
    public record TrendPoint(string Month, int Active, int Pending, int Rejected, int Expired, int Closed);
    public record NameCount(string Label, int Count);

    // ----------------------------------------------------------------- helpers
    private async Task<ExceptionEvidence> SaveEvidenceAsync(IFormFile file, string? evidenceType, int? userId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "evidence");
        Directory.CreateDirectory(dir);
        var stored = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        await using var stream = new FileStream(Path.Combine(dir, stored), FileMode.Create);
        await file.CopyToAsync(stream);
        return new ExceptionEvidence
        {
            EvidenceType = Enum.TryParse<EvidenceType>(evidenceType, out var t) ? t : EvidenceType.Other,
            OriginalFileName = file.FileName,
            StoredFileName = stored,
            SizeBytes = file.Length,
            UploadedByUserId = userId,
            UploadedAt = DateTime.UtcNow
        };
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) && uid >= 0 ? uid : null;

    private ICollection<string> CurrentRoles() =>
        User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
}
