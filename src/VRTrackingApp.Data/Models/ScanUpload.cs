using System;

namespace VRTrackingApp.Data.Models;

public class ScanUpload
{
    public int Id { get; set; }
    public string FileName { get; set; } = default!;
    public string? FileHash { get; set; }
    public long FileSize { get; set; }
    public string Status { get; set; } = "Pending";

    // Scan cycle metadata (Monthly / Patch Tuesday / Zero Day / Risk-based)
    public string? ScanCycleLabel { get; set; }
    public DateTime? ScanDate { get; set; }
    public string SourceType { get; set; } = "Monthly";
    public string? Notes { get; set; }

    public int? UploadedByUserId { get; set; }
    public UserAccount? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetHost> AssetHosts { get; set; } = new List<AssetHost>();
    public ICollection<UploadAuditTrail> AuditTrail { get; set; } = new List<UploadAuditTrail>();
}
