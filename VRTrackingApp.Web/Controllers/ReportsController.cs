using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Html;
using VRTrackingApp.Web.Services;

namespace VRTrackingApp.Web.Controllers;

public class ReportsController : Controller
{
    private readonly VRTrackingAppContext _db;
    public ReportsController(VRTrackingAppContext db) => _db = db;

    public async Task<IActionResult> Index(string type = "Monthly")
    {
        ViewData["Title"] = "Reports";
        ViewBag.ReportType = type;

        var instances = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .Include(i => i.Owner)
            .ToListAsync();

        // Report-type specific slicing
        var filtered = type switch
        {
            "ZeroDay" => instances.Where(i => i.VulnerabilityFinding!.Severity is "Critical" or "High"
                                              && (i.AssetHost?.ScanUpload?.ScanDate ?? DateTime.MinValue) >= DateTime.UtcNow.AddDays(-30)).ToList(),
            "RiskBased" => instances.OrderByDescending(i => i.VulnerabilityFinding!.CvssV3BaseScore ?? i.VulnerabilityFinding!.VprScore ?? 0).Take(50).ToList(),
            "PatchTuesday" => instances.Where(i => i.AssetHost?.ScanUpload?.SourceType == "Patch Tuesday").ToList(),
            _ => instances
        };

        // Remediation by month (scan cycle)
        var byMonth = filtered
            .Where(i => i.AssetHost?.ScanUpload?.ScanDate != null)
            .GroupBy(i => i.AssetHost!.ScanUpload!.ScanDate!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => ToMonthStat(g))
            .ToList();

        // Open by severity
        var openBySev = new[] { "Critical", "High", "Medium", "Low", "Info" }
            .ToDictionary(s => s, s => filtered.Count(i => i.Status == "Open" && i.VulnerabilityFinding!.Severity == s));

        // Exceptions by team (owner)
        var excByTeam = filtered.Where(i => i.Status == "Exception")
            .GroupBy(i => i.Owner?.DisplayName ?? "Unassigned")
            .ToDictionary(g => g.Key, g => g.Count());

        // Cycle-to-cycle trend (fixed %)
        var trend = byMonth.Select(m => new TrendStat(m.Month,
            m.Open + m.Fixed + m.Exception,
            m.Fixed + m.Exception == 0 ? 0 : (int)(100.0 * (m.Fixed + m.Exception) / (m.Open + m.Fixed + m.Exception)))
        ).ToList();

        // ---- Exception metrics (P7) ----
        var excs = await _db.ExceptionRecords
            .Include(e => e.VulnerabilityInstance).ThenInclude(i => i.VulnerabilityFinding)
            .Include(e => e.Owner)
            .ToListAsync();
        var now = DateTime.UtcNow;
        bool IsActive(ExceptionStatus s) => s is ExceptionStatus.ActiveException or ExceptionStatus.Renewed;
        bool IsPending(ExceptionStatus s) => s is ExceptionStatus.PendingTechnicalApproval
            or ExceptionStatus.PendingManagerApproval or ExceptionStatus.PendingSecurityApproval;

        var acceptedCritical = excs.Count(e => IsActive(e.Status)
            && e.VulnerabilityInstance?.VulnerabilityFinding?.Severity == "Critical");
        var avgAgeDays = excs.Where(e => IsActive(e.Status) && e.CreatedAt != default)
            .Select(e => (now - e.CreatedAt).TotalDays).DefaultIfEmpty(0).Average();
        var byReason = excs
            .Where(e => e.NonFixableReason.HasValue)
            .GroupBy(e => e.NonFixableReason!.Value)
            .Select(g => new NameCount(g.Key.Humanize(), g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        ViewBag.ExcSummary = new ExcSummary(
            excs.Count,
            excs.Count(e => IsActive(e.Status)),
            excs.Count(e => IsPending(e.Status)),
            excs.Count(e => e.Status == ExceptionStatus.Expired),
            excs.Count(e => e.Status == ExceptionStatus.Rejected),
            excs.Count(e => e.Status == ExceptionStatus.ReviewDue),
            acceptedCritical,
            (int)Math.Round(avgAgeDays));
        ViewBag.ExcByReason = byReason;

        ViewBag.ByMonth = byMonth;
        ViewBag.OpenBySev = openBySev;
        ViewBag.ExcByTeam = excByTeam;
        ViewBag.Trend = trend;
        ViewBag.Filtered = filtered;
        return View();
    }

    public async Task<IActionResult> ExportCsv()
    {
        var instances = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost)
            .Include(i => i.Owner)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Host,IP,Severity,PluginId,Finding,CVE,Status,Owner,DueDate");
        foreach (var i in instances.OrderBy(i => i.AssetHost!.HostName))
        {
            sb.AppendLine(string.Join(",",
                Csv(i.AssetHost?.HostName), Csv(i.AssetHost?.IpAddress),
                Csv(i.VulnerabilityFinding?.Severity), Csv(i.VulnerabilityFinding?.PluginId.ToString()),
                Csv(i.VulnerabilityFinding?.PluginName), Csv(i.VulnerabilityFinding?.Cve),
                Csv(i.Status), Csv(i.Owner?.DisplayName), Csv(i.DueDate?.ToString("yyyy-MM-dd"))));
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"vr_remediation_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportPdf(string type = "Monthly")
    {
        var instances = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .Include(i => i.Owner)
            .ToListAsync();

        var filtered = type switch
        {
            "ZeroDay" => instances.Where(i => i.VulnerabilityFinding!.Severity is "Critical" or "High"
                                              && (i.AssetHost?.ScanUpload?.ScanDate ?? DateTime.MinValue) >= DateTime.UtcNow.AddDays(-30)).ToList(),
            "RiskBased" => instances.OrderByDescending(i => i.VulnerabilityFinding!.CvssV3BaseScore ?? i.VulnerabilityFinding!.VprScore ?? 0).Take(50).ToList(),
            "PatchTuesday" => instances.Where(i => i.AssetHost?.ScanUpload?.SourceType == "Patch Tuesday").ToList(),
            _ => instances
        };

        var lines = new List<string>
        {
            $"Vulnerability Remediation Report - {type}",
            $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
            $"Total findings in scope: {filtered.Count}",
            ""
        };
        foreach (var s in new[] { "Critical", "High", "Medium", "Low", "Info" })
        {
            var c = filtered.Count(i => i.VulnerabilityFinding!.Severity == s);
            if (c > 0) lines.Add($"  {s}: {c}");
        }
        lines.Add("");
        lines.Add("Open findings by host:");
        foreach (var g in filtered.Where(i => i.Status == "Open").GroupBy(i => i.AssetHost!.HostName).OrderBy(g => g.Key))
            lines.Add($"  {g.Key}: {g.Count()}");

        var bytes = PdfWriter.Write($"VR Report - {type}", lines);
        return File(bytes, "application/pdf", $"vr_report_{type}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private static string Csv(string? v) => $"\"{(v ?? "").Replace("\"", "\"\"")}\"";

    // ----------------------------------------------------------------- Vulnerability trend analytics (P8)
    [HttpGet]
    public async Task<IActionResult> Trends()
    {
        ViewData["Title"] = "Vulnerability Trends";
        var instances = await _db.VulnerabilityInstances
            .Include(i => i.VulnerabilityFinding)
            .Include(i => i.AssetHost).ThenInclude(h => h.ScanUpload)
            .ToListAsync();

        var sevCycle = instances
            .Where(i => i.AssetHost?.ScanUpload?.ScanDate != null)
            .GroupBy(i => i.AssetHost!.ScanUpload!.ScanDate!.Value.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new SeverityTrendPoint(
                g.Key,
                g.Count(i => i.VulnerabilityFinding!.Severity == "Critical"),
                g.Count(i => i.VulnerabilityFinding!.Severity == "High"),
                g.Count(i => i.VulnerabilityFinding!.Severity == "Medium"),
                g.Count(i => i.VulnerabilityFinding!.Severity == "Low"),
                g.Count(i => i.VulnerabilityFinding!.Severity is "Info" or "None")))
            .ToList();

        var totalNow = instances.Count;
        var openNow = instances.Count(i => i.Status == "Open");
        var fixedNow = instances.Count(i => i.Status == "Fixed");
        var excNow = instances.Count(i => i.Status == "Exception");
        ViewBag.SevCycle = sevCycle;
        ViewBag.Totals = new TrendTotals(totalNow, openNow, fixedNow, excNow);
        return View();
    }

    public record SeverityTrendPoint(string Month, int Critical, int High, int Medium, int Low, int Info);
    public record TrendTotals(int Total, int Open, int Fixed, int Exception);
    public record MonthStat(string Month, int Open, int Fixed, int Exception);
    public record TrendStat(string Month, int Total, int RemediatedPct);
    public record ExcSummary(int Total, int Active, int Pending, int Expired, int Rejected, int ReviewDue, int AcceptedCritical, int AvgAgeDays);
    public record NameCount(string Label, int Count);

    private static MonthStat ToMonthStat(IGrouping<string, VulnerabilityInstance> g)
        => new MonthStat(
            g.Key,
            g.Count(i => i.Status == "Open"),
            g.Count(i => i.Status == "Fixed"),
            g.Count(i => i.Status == "Exception"));
}
