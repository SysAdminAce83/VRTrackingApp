using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services;
using VRTrackingApp.Web.Services.Notifications;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin,Analyst,Remediation Owner")]
public class UploadController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly ScanImportService _import;
    private readonly ScanIngestionService _ingest;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] Sources = { "Monthly", "Quarterly", "On-demand", "Per Server", "Per Application", "Per Business Unit" };

    public UploadController(VRTrackingAppContext db, ScanImportService import,
        ScanIngestionService ingest, IWebHostEnvironment env)
    {
        _db = db; _import = import; _ingest = ingest; _env = env;
    }

    private int? CurrentUserId() =>
        int.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var uid) && uid >= 0 ? uid : null;

    public IActionResult Index()
    {
        ViewData["Title"] = "Upload Scan";
        ViewBag.SourceTypes = Sources;
        return View();
    }

    private static string FormatOf(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".csv" => "csv",
            ".pdf" => "pdf",
            ".nessus" or ".xml" => "nessus",
            ".txt" => "txt",
            _ => "unknown"
        };

    // Step 1: store + hash + parse preview + run dedup decision (Scenario 1-4).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(IFormFile file, string? scanName, string? cycleLabel,
        DateTime? scanDate, string sourceType = "Monthly", string? notes = null)
    {
        ViewData["Title"] = "Upload Scan";
        ViewBag.SourceTypes = Sources;

        if (file == null || file.Length == 0) { ViewBag.Error = "No file was provided."; return View(nameof(Index)); }
        if (!_import.IsAllowedFile(file.FileName, file.Length, file.ContentType))
        {
            ViewBag.Error = "Only .csv, .pdf, .nessus or .txt reports up to 100 MB are allowed.";
            return View(nameof(Index));
        }

        var fmt = FormatOf(file.FileName);
        var ext = "." + (fmt == "nessus" ? "nessus" : fmt);
        var token = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(dir);
        var storedPath = Path.Combine(dir, token);
        await using (var fs = System.IO.File.Create(storedPath))
            await file.CopyToAsync(fs);

        // Hashing (Scenario 3).
        string sha, md5;
        using (var fs2 = System.IO.File.OpenRead(storedPath))
            (sha, md5) = await _ingest.ComputeHashesAsync(fs2);

        // Parse to normalized model (Scenario 8).
        ParsedScan parsed;
        await using (var stream = System.IO.File.OpenRead(storedPath))
        {
            parsed = fmt switch
            {
                "csv" => await _import.ParseCsvToModelAsync(stream),
                "pdf" => await _import.ParsePdfToModelAsync(stream),
                "nessus" => await NessusXmlParser.ParseAsync(stream),
                _ => await _import.ParseTxtToModelAsync(stream)
            };
        }

        if (!parsed.Valid)
        {
            ViewBag.Error = parsed.Message;
            ViewBag.ValidationChecks = parsed.ValidationChecks;
            return View(nameof(Index));
        }

        // Dedup decision (Scenario 1,2,3,4,11).
        var scanKey = ScanIngestionService.ComputeScanKey(parsed.Metadata);
        var decision = await _ingest.DecideAsync(sha, scanKey, CurrentUserId() ?? 0);

        var vm = new UploadPreviewVm
        {
            Token = token,
            OriginalFileName = file.FileName,
            FileHash = sha,
            Md5Hash = md5,
            FileSize = file.Length,
            Format = fmt,
            ScanKey = scanKey,
            ScanName = scanName,
            CycleLabel = cycleLabel,
            ScanDate = scanDate ?? parsed.Metadata?.ScanStart,
            SourceType = sourceType,
            Notes = notes,
            Preview = parsed,
            Decision = decision
        };
        return View("Preview", vm);
    }

    // Step 2: commit the previously validated file through the ingestion engine.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(UploadPreviewVm vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Token) ||
            !System.Text.RegularExpressions.Regex.IsMatch(vm.Token, @"^[0-9a-fA-F]{32}\.(csv|pdf|nessus|txt)$"))
            return BadRequest("Invalid token.");

        var storedPath = Path.Combine(_env.ContentRootPath, "Uploads", vm.Token);
        if (!System.IO.File.Exists(storedPath)) return NotFound("Pending upload not found.");

        var userId = CurrentUserId() ?? 0;
        var fmt = vm.Format ?? FormatOf(vm.OriginalFileName ?? vm.Token);

        var scan = new ScanUpload
        {
            FileName = vm.OriginalFileName ?? vm.Token,
            FileHash = vm.FileHash,
            Md5Hash = vm.Md5Hash,
            FileSize = vm.FileSize,
            Format = fmt,
            Status = "Processing",
            ScanCycleLabel = string.IsNullOrWhiteSpace(vm.ScanName) ? vm.CycleLabel : vm.ScanName,
            ScanDate = vm.ScanDate,
            SourceType = vm.SourceType ?? "Monthly",
            Notes = vm.Notes,
            UploadedByUserId = userId,
            UploadedAt = DateTime.UtcNow
        };
        _db.ScanUploads.Add(scan);
        await _db.SaveChangesAsync();

        // Re-parse for ingestion.
        ParsedScan parsed;
        await using (var stream = System.IO.File.OpenRead(storedPath))
        {
            parsed = fmt switch
            {
                "csv" => await _import.ParseCsvToModelAsync(stream),
                "pdf" => await _import.ParsePdfToModelAsync(stream),
                "nessus" => await NessusXmlParser.ParseAsync(stream),
                _ => await _import.ParseTxtToModelAsync(stream)
            };
        }

        var result = await _ingest.IngestAsync(scan, parsed, userId);
        System.IO.File.Delete(storedPath);

        TempData["ImportMessage"] = result.Errors.Count == 0
            ? $"{result.Outcome}: {result.NewCount} new, {result.ExistingCount} existing, {result.ReopenedCount} reopened."
            : string.Join("; ", result.Errors.Take(5));
        return RedirectToAction("Details", "Scans", new { id = scan.Id });
    }

    public class UploadPreviewVm
    {
        public string Token { get; set; } = "";
        public string? OriginalFileName { get; set; }
        public string? FileHash { get; set; }
        public string? Md5Hash { get; set; }
        public long FileSize { get; set; }
        public string? Format { get; set; }
        public string? ScanKey { get; set; }
        public string? ScanName { get; set; }
        public string? CycleLabel { get; set; }
        public DateTime? ScanDate { get; set; }
        public string? SourceType { get; set; }
        public string? Notes { get; set; }
        public ParsedScan Preview { get; set; } = new();
        public IngestionDecision? Decision { get; set; }
    }
    // ---- Scan Groups (Scenario 4, 5, 6, 9) ----
    public async Task<IActionResult> Groups()
    {
        ViewData["Title"] = "Scan Groups";
        var groups = await _db.ScanGroups
            .Include(g => g.Uploads).ThenInclude(u => u.UploadedBy)
            .Include(g => g.Uploads).ThenInclude(u => u.IngestionAudit)
            .OrderByDescending(g => g.UpdatedAt)
            .ToListAsync();
        return View(groups);
    }

    public async Task<IActionResult> Group(int id)
    {
        var group = await _db.ScanGroups
            .Include(g => g.Uploads).ThenInclude(u => u.UploadedBy)
            .Include(g => g.Uploads).ThenInclude(u => u.IngestionAudit)
            .Include(g => g.Uploads).ThenInclude(u => u.DeduplicationLogs)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();
        ViewData["Title"] = $"Scan Group #{id}";
        return View(group);
    }

    // Per-upload ingestion audit (Scenario 13).
    public async Task<IActionResult> History(int id)
    {
        var upload = await _db.ScanUploads
            .Include(u => u.IngestionAudit)
            .Include(u => u.DeduplicationLogs)
            .Include(u => u.UploadedBy)
            .Include(u => u.ScanGroup)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (upload == null) return NotFound();
        ViewData["Title"] = $"Ingestion audit — {upload.FileName}";
        return View(upload);
    }
}
