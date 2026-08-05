using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

[Authorize]
public class AssetController : Controller
{
    private readonly VRTrackingAppContext _db;
    public AssetController(VRTrackingAppContext db) => _db = db;

    private static readonly string[] SuggestedCategories =
    {
        "Server", "Workstation", "Database", "Network Device", "Security Appliance",
        "Application", "Storage", "Virtual Machine", "Cloud Resource", "Other"
    };
    private static readonly string[] SuggestedSubCategories =
    {
        "Web Server", "Database Server", "Domain Controller", "Application Server",
        "File Server", "Domain Member", "Firewall", "Load Balancer", "Router", "Switch",
        "Endpoint", "Hypervisor", "Container Host", "Generic Server"
    };

    // Properties that should not be tracked as audit "field changes".
    private static readonly HashSet<string> SkipProps = new()
    {
        nameof(Asset.Id), nameof(Asset.ScanHosts),
        nameof(Asset.FirstSeen), nameof(Asset.LastSeen),
        nameof(Asset.CreatedAt), nameof(Asset.UpdatedAt)
    };

    public class AssetRow
    {
        public Asset Asset { get; set; } = default!;
        public int Critical { get; set; }
        public int High { get; set; }
        public int Open { get; set; }
        public int Total { get; set; }
    }

    public async Task<IActionResult> Index(string? q, string? category, string? datacenter, string? status)
    {
        ViewData["Title"] = "Assets";
        ViewBag.Category = category;
        ViewBag.Datacenter = datacenter;
        ViewBag.Status = status;
        ViewBag.Categories = await _db.Assets
            .Where(a => !string.IsNullOrEmpty(a.Category))
            .Select(a => a.Category!).Distinct().OrderBy(c => c).ToListAsync();
        ViewBag.Datacenters = await _db.Assets
            .Where(a => !string.IsNullOrEmpty(a.Datacenter))
            .Select(a => a.Datacenter!).Distinct().OrderBy(d => d).ToListAsync();

        var assets = _db.Assets
            .Include(a => a.ScanHosts).ThenInclude(h => h.Instances).ThenInclude(i => i.VulnerabilityFinding)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            assets = assets.Where(a => a.Category == category);
        if (!string.IsNullOrWhiteSpace(datacenter))
            assets = assets.Where(a => a.Datacenter == datacenter);
        if (!string.IsNullOrWhiteSpace(status))
            assets = assets.Where(a => a.AssetStatus == status);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = q.Trim().ToLower();
            assets = assets.Where(a =>
                a.HostName.ToLower().Contains(t) ||
                a.IpAddress.ToLower().Contains(t) ||
                (a.Location != null && a.Location.ToLower().Contains(t)) ||
                (a.Category != null && a.Category.ToLower().Contains(t)) ||
                (a.Datacenter != null && a.Datacenter.ToLower().Contains(t)));
        }

        var rows = new List<AssetRow>();
        foreach (var a in await assets.OrderBy(x => x.HostName).ToListAsync())
        {
            var crit = a.ScanHosts.SelectMany(h => h.Instances)
                .Count(i => i.VulnerabilityFinding!.Severity == "Critical" && i.Status == "Open");
            var high = a.ScanHosts.SelectMany(h => h.Instances)
                .Count(i => i.VulnerabilityFinding!.Severity == "High" && i.Status == "Open");
            var open = a.ScanHosts.SelectMany(h => h.Instances).Count(i => i.Status == "Open");
            var total = a.ScanHosts.SelectMany(h => h.Instances).Count();
            rows.Add(new AssetRow { Asset = a, Critical = crit, High = high, Open = open, Total = total });
        }

        ViewBag.Total = rows.Count;
        ViewBag.OpenCount = rows.Sum(r => r.Open);
        return View(rows);
    }

    public async Task<IActionResult> Details(int id)
    {
        var asset = await _db.Assets
            .Include(a => a.ScanHosts).ThenInclude(h => h.ScanUpload)
            .Include(a => a.ScanHosts).ThenInclude(h => h.Instances).ThenInclude(i => i.VulnerabilityFinding)
            .Include(a => a.ScanHosts).ThenInclude(h => h.Instances).ThenInclude(i => i.Owner)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return NotFound();

        ViewData["Title"] = asset.HostName;
        return View(asset);
    }

    // ---- Manual device entry -------------------------------------------------
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            ViewData["Title"] = "Add device";
            ViewBag.SuggestedCategories = SuggestedCategories.Select(TitleCase).ToArray();
            ViewBag.SuggestedSubCategories = SuggestedSubCategories.Select(TitleCase).ToArray();
            return View(new Asset());
        }
        catch (Exception ex)
        {
            return Content(ex.ToString());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(
        "ServerId,HostName,IpAddress,AssetStatus,Datacenter,Location,OperatingSystem," +
        "BiaCriticality,Confidentiality,Integrity,Availability,AssetCriticality," +
        "Application,ApplicationOwner,ActiveDirectoryComments,Environment,BackupDocument,BackupOwner,GroupLandscape,Category,SubCategory,ApplicationSystemName,SystemDescription," +
        "AssetOwner,BusinessOwner,InternalPoc,OnsiteResourceBackup,PocRole,ExternalPoc,ExternalPocRole," +
        "CpuCoreCount,RamGb,DriveCount,TotalDiskSpaceGb,Vendor,HardwareDescription," +
        "Hardware,Software,ServerName2,ServerIpAddress2,OtherItResources,RedundancyPrimaryDc,CriticalRoles,CriticalResources," +
        "InterDependencies,DataReplicationFrequency,PeakTime,OffPeakTime,OutageImpact,FinancialImpact,NonFinancialImpact,RegulatoryImpact,Criticality2," +
        "DisasterRecoveryRequired,DrSetupDetails,RedundancyDrDc,PrioritizeResourceRecovery,MinimumHardware,MinimumHardware2,MinimumItResources,Rto,Rpo,Mol,Remarks,BackupSchedule,BusUnit,Notes"
        )] Asset model)
    {
        ViewBag.SuggestedCategories = SuggestedCategories.Select(TitleCase).ToArray();
        ViewBag.SuggestedSubCategories = SuggestedSubCategories.Select(TitleCase).ToArray();

        if (string.IsNullOrWhiteSpace(model.HostName) || string.IsNullOrWhiteSpace(model.IpAddress))
        {
            ModelState.AddModelError("", "Hostname and IP address are required.");
            return View(model);
        }

        model.Category = TitleCase(model.Category);
        model.SubCategory = TitleCase(model.SubCategory);
        model.FirstSeen = DateTime.UtcNow;
        model.LastSeen = DateTime.UtcNow;
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;

        _db.Assets.Add(model);
        await _db.SaveChangesAsync();

        var changes = new List<AssetFieldChange>
        {
            new() { Field = "HostName", NewValue = model.HostName },
            new() { Field = "IpAddress", NewValue = model.IpAddress },
            new() { Field = "Category", NewValue = model.Category },
            new() { Field = "SubCategory", NewValue = model.SubCategory }
        };
        await LogAssetAuditAsync(model.Id, "Created", changes,
            $"Manual entry · {model.HostName} ({model.IpAddress})");

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // ---- Edit ----------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _db.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        ViewData["Title"] = $"Edit {asset.HostName}";
        ViewBag.SuggestedCategories = SuggestedCategories.Select(TitleCase).ToArray();
        ViewBag.SuggestedSubCategories = SuggestedSubCategories.Select(TitleCase).ToArray();
        return View(asset);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(
        "ServerId,HostName,IpAddress,AssetStatus,Datacenter,Location,OperatingSystem," +
        "BiaCriticality,Confidentiality,Integrity,Availability,AssetCriticality," +
        "Application,ApplicationOwner,ActiveDirectoryComments,Environment,BackupDocument,BackupOwner,GroupLandscape,Category,SubCategory,ApplicationSystemName,SystemDescription," +
        "AssetOwner,BusinessOwner,InternalPoc,OnsiteResourceBackup,PocRole,ExternalPoc,ExternalPocRole," +
        "CpuCoreCount,RamGb,DriveCount,TotalDiskSpaceGb,Vendor,HardwareDescription," +
        "Hardware,Software,ServerName2,ServerIpAddress2,OtherItResources,RedundancyPrimaryDc,CriticalRoles,CriticalResources," +
        "InterDependencies,DataReplicationFrequency,PeakTime,OffPeakTime,OutageImpact,FinancialImpact,NonFinancialImpact,RegulatoryImpact,Criticality2," +
        "DisasterRecoveryRequired,DrSetupDetails,RedundancyDrDc,PrioritizeResourceRecovery,MinimumHardware,MinimumHardware2,MinimumItResources,Rto,Rpo,Mol,Remarks,BackupSchedule,BusUnit,Notes"
        )] Asset model)
    {
        var asset = await _db.Assets.FindAsync(id);
        if (asset == null) return NotFound();

        var original = Snapshot(asset);

        asset.ServerId = model.ServerId;
        asset.HostName = model.HostName;
        asset.IpAddress = model.IpAddress;
        asset.AssetStatus = model.AssetStatus;
        asset.Datacenter = model.Datacenter;
        asset.Location = model.Location;
        asset.OperatingSystem = model.OperatingSystem;
        asset.BiaCriticality = model.BiaCriticality;
        asset.Confidentiality = model.Confidentiality;
        asset.Integrity = model.Integrity;
        asset.Availability = model.Availability;
        asset.AssetCriticality = model.AssetCriticality;
        asset.Application = model.Application;
        asset.ApplicationOwner = model.ApplicationOwner;
        asset.ActiveDirectoryComments = model.ActiveDirectoryComments;
        asset.Environment = model.Environment;
        asset.BackupDocument = model.BackupDocument;
        asset.BackupOwner = model.BackupOwner;
        asset.GroupLandscape = model.GroupLandscape;
        asset.Category = TitleCase(model.Category);
        asset.SubCategory = TitleCase(model.SubCategory);
        asset.ApplicationSystemName = model.ApplicationSystemName;
        asset.SystemDescription = model.SystemDescription;
        asset.AssetOwner = model.AssetOwner;
        asset.BusinessOwner = model.BusinessOwner;
        asset.InternalPoc = model.InternalPoc;
        asset.OnsiteResourceBackup = model.OnsiteResourceBackup;
        asset.PocRole = model.PocRole;
        asset.ExternalPoc = model.ExternalPoc;
        asset.ExternalPocRole = model.ExternalPocRole;
        asset.CpuCoreCount = model.CpuCoreCount;
        asset.RamGb = model.RamGb;
        asset.DriveCount = model.DriveCount;
        asset.TotalDiskSpaceGb = model.TotalDiskSpaceGb;
        asset.Vendor = model.Vendor;
        asset.HardwareDescription = model.HardwareDescription;
        asset.Hardware = model.Hardware;
        asset.Software = model.Software;
        asset.ServerName2 = model.ServerName2;
        asset.ServerIpAddress2 = model.ServerIpAddress2;
        asset.OtherItResources = model.OtherItResources;
        asset.RedundancyPrimaryDc = model.RedundancyPrimaryDc;
        asset.CriticalRoles = model.CriticalRoles;
        asset.CriticalResources = model.CriticalResources;
        asset.InterDependencies = model.InterDependencies;
        asset.DataReplicationFrequency = model.DataReplicationFrequency;
        asset.PeakTime = model.PeakTime;
        asset.OffPeakTime = model.OffPeakTime;
        asset.OutageImpact = model.OutageImpact;
        asset.FinancialImpact = model.FinancialImpact;
        asset.NonFinancialImpact = model.NonFinancialImpact;
        asset.RegulatoryImpact = model.RegulatoryImpact;
        asset.Criticality2 = model.Criticality2;
        asset.DisasterRecoveryRequired = model.DisasterRecoveryRequired;
        asset.DrSetupDetails = model.DrSetupDetails;
        asset.RedundancyDrDc = model.RedundancyDrDc;
        asset.PrioritizeResourceRecovery = model.PrioritizeResourceRecovery;
        asset.MinimumHardware = model.MinimumHardware;
        asset.MinimumHardware2 = model.MinimumHardware2;
        asset.MinimumItResources = model.MinimumItResources;
        asset.Rto = model.Rto;
        asset.Rpo = model.Rpo;
        asset.Mol = model.Mol;
        asset.Remarks = model.Remarks;
        asset.BackupSchedule = model.BackupSchedule;
        asset.BusUnit = model.BusUnit;
        asset.Notes = model.Notes;
        asset.UpdatedAt = DateTime.UtcNow;

        var changes = ComputeChanges(original, asset);
        await _db.SaveChangesAsync();

        if (changes.Count > 0)
        {
            await LogAssetAuditAsync(asset.Id, "Updated", changes,
                $"{changes.Count} field(s) changed: {string.Join(", ", changes.Select(c => c.Field))}");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var asset = await _db.Assets.FindAsync(id);
        if (asset == null) return NotFound();
        ViewData["Title"] = $"Delete {asset.HostName}";
        return View(asset);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var asset = await _db.Assets.FindAsync(id);
        if (asset != null)
        {
            var host = asset.HostName;
            var ip = asset.IpAddress;
            _db.Assets.Remove(asset);
            await _db.SaveChangesAsync();

            // Audit row kept after the asset is gone (AssetId is set to null by the FK).
            await LogAssetAuditAsync(null, "Deleted",
                details: $"{host} ({ip}) removed from inventory");
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Helpers -------------------------------------------------------------
    private int? GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task LogAssetAuditAsync(int? assetId, string action,
        List<AssetFieldChange>? changes = null, string? details = null)
    {
        _db.AssetAuditTrails.Add(new AssetAuditTrail
        {
            AssetId = assetId,
            Action = action,
            PerformedByUserId = GetCurrentUserId(),
            PerformedAt = DateTime.UtcNow,
            Details = details,
            FieldChanges = changes ?? new List<AssetFieldChange>()
        });
        await _db.SaveChangesAsync();
    }

    private static Asset Snapshot(Asset source)
    {
        var copy = new Asset();
        foreach (var p in typeof(Asset).GetProperties())
            if (p.CanRead && p.CanWrite) p.SetValue(copy, p.GetValue(source));
        return copy;
    }

    private static List<AssetFieldChange> ComputeChanges(Asset original, Asset updated)
    {
        var changes = new List<AssetFieldChange>();
        foreach (var p in typeof(Asset).GetProperties())
        {
            if (!p.CanRead || !p.CanWrite || SkipProps.Contains(p.Name)) continue;
            var o = p.GetValue(original);
            var n = p.GetValue(updated);
            if (!Equals(o, n))
            {
                changes.Add(new AssetFieldChange
                {
                    Field = p.Name,
                    OldValue = o == null ? null : Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture),
                    NewValue = n == null ? null : Convert.ToString(n, System.Globalization.CultureInfo.InvariantCulture)
                });
            }
        }
        return changes;
    }

    private static string TitleCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value ?? "";
        var trimmed = value.Trim();
        return char.ToUpperInvariant(trimmed[0]) + trimmed[1..].ToLowerInvariant();
    }
}
