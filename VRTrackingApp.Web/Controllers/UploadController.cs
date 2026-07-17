using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin,Analyst,Remediation Owner")]
public class UploadController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly ScanImportService _import;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] Sources = { "Monthly", "Patch Tuesday", "Zero Day", "Risk-based" };

    public UploadController(VRTrackingAppContext db, ScanImportService import, IWebHostEnvironment env)
    {
        _db = db; _import = import; _env = env;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Upload Scan";
        ViewBag.SourceTypes = Sources;
        return View();
    }

    // Step 1: validate + parse into a preview WITHOUT saving.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(IFormFile file, string? scanName, string? cycleLabel,
        DateTime? scanDate, string sourceType = "Monthly", string? notes = null)
    {
        ViewData["Title"] = "Upload Scan";
        ViewBag.SourceTypes = Sources;

        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "No file was provided.";
            return View(nameof(Index));
        }
        if (!_import.IsAllowedFile(file.FileName, file.Length, file.ContentType))
        {
            ViewBag.Error = "Only .csv, .pdf or .txt reports up to 100 MB are allowed.";
            return View(nameof(Index));
        }

        // Store with a random server-generated name — never trust the client file name.
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var token = $"{Guid.NewGuid():N}{ext}";
        var dir = Path.Combine(_env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(dir);
        var storedPath = Path.Combine(dir, token);
        using (var fs = System.IO.File.Create(storedPath))
            await file.CopyToAsync(fs);

        string hash;
        using (var sha = System.Security.Cryptography.SHA256.Create())
        using (var hs = System.IO.File.OpenRead(storedPath))
            hash = BitConverter.ToString(await sha.ComputeHashAsync(hs)).Replace("-", "").ToLowerInvariant();

        ScanPreview preview;
        if (ext == ".csv")
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            preview = await _import.PreviewCsvAsync(stream);
        }
        else if (ext == ".txt")
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            preview = await _import.PreviewTxtAsync(stream);
        }
        else
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            preview = await _import.PreviewPdfAsync(stream);
        }

        var vm = new UploadPreviewVm
        {
            Token = token,
            OriginalFileName = file.FileName,
            FileHash = hash,
            FileSize = file.Length,
            ScanName = scanName,
            CycleLabel = cycleLabel,
            ScanDate = scanDate,
            SourceType = sourceType,
            Notes = notes,
            Preview = preview
        };
        return View("Preview", vm);
    }

    // Step 2: commit the previously validated file.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(UploadPreviewVm vm)
    {
        // Validate token to prevent path traversal.
        if (string.IsNullOrWhiteSpace(vm.Token) || !System.Text.RegularExpressions.Regex.IsMatch(vm.Token, "^[0-9a-fA-F]{32}\\.(csv|pdf)$"))
            return BadRequest("Invalid token.");

        var storedPath = Path.Combine(_env.ContentRootPath, "Uploads", vm.Token);
        if (!System.IO.File.Exists(storedPath)) return NotFound("Pending upload not found.");

        var ext = Path.GetExtension(vm.Token).ToLowerInvariant();
        var scan = new ScanUpload
        {
            FileName = vm.OriginalFileName ?? vm.Token,
            FileHash = vm.FileHash,
            FileSize = vm.FileSize,
            Status = "Processing",
            ScanCycleLabel = string.IsNullOrWhiteSpace(vm.ScanName) ? vm.CycleLabel : vm.ScanName,
            ScanDate = vm.ScanDate,
            SourceType = vm.SourceType ?? "Monthly",
            Notes = vm.Notes,
            UploadedAt = DateTime.UtcNow
        };
        _db.ScanUploads.Add(scan);
        await _db.SaveChangesAsync();

        ScanParseResult result;
        if (ext == ".csv")
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            result = await _import.ImportCsvAsync(stream, scan, vm.OriginalFileName ?? vm.Token);
            scan.Status = result.Success ? "Completed" : "Failed";
        }
        else if (ext == ".txt")
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            result = await _import.ImportTxtAsync(stream, scan);
            scan.Status = result.Success ? "Completed" : "Failed";
        }
        else
        {
            await using var stream = System.IO.File.OpenRead(storedPath);
            result = await _import.ImportPdfAsync(stream, scan);
            scan.Status = result.Success ? "Completed" : "Awaiting Parsing";
        }

        _db.UploadAuditTrails.Add(new UploadAuditTrail
        {
            ScanUploadId = scan.Id,
            Action = result.Success ? "Imported & committed" : "Import failed",
            PerformedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["ImportMessage"] = result.Message;
        return RedirectToAction("Details", "Scans", new { id = scan.Id });
    }

    public class UploadPreviewVm
    {
        public string Token { get; set; } = "";
        public string? OriginalFileName { get; set; }
        public string? FileHash { get; set; }
        public long FileSize { get; set; }
        public string? ScanName { get; set; }
        public string? CycleLabel { get; set; }
        public DateTime? ScanDate { get; set; }
        public string? SourceType { get; set; }
        public string? Notes { get; set; }
        public ScanPreview Preview { get; set; } = new();
    }
}
