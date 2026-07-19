using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class ScanUpload
{
    public int Id { get; set; }
    public string FileName { get; set; } = default!;

    // ----- File-level identity (Scenario 3) -----
    /// <summary>SHA-256 of the raw uploaded bytes. Primary byte-for-byte duplicate detector.</summary>
    public string? FileHash { get; set; }
    /// <summary>MD5 kept for interoperability with legacy Nessus tooling / quick checksums.</summary>
    public string? Md5Hash { get; set; }
    public long FileSize { get; set; }
    /// <summary>Detected report format: csv | pdf | nessus | txt</summary>
    public string? Format { get; set; }

    public string Status { get; set; } = "Pending";

    // ----- Scan-level grouping (Scenario 4) -----
    public int? ScanGroupId { get; set; }
    public ScanGroup? ScanGroup { get; set; }

    // Scan cycle metadata (Monthly / Patch Tuesday / Zero Day / Risk-based)
    public string? ScanCycleLabel { get; set; }
    public DateTime? ScanDate { get; set; }
    public string SourceType { get; set; } = "Monthly";
    public string? Notes { get; set; }

    public int? UploadedByUserId { get; set; }
    public UserAccount? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ScanMetadata? Metadata { get; set; }
    public IngestionAudit? IngestionAudit { get; set; }

    public ICollection<AssetHost> AssetHosts { get; set; } = new List<AssetHost>();
    public ICollection<UploadAuditTrail> AuditTrail { get; set; } = new List<UploadAuditTrail>();
    public ICollection<DeduplicationLog> DeduplicationLogs { get; set; } = new List<DeduplicationLog>();
}
