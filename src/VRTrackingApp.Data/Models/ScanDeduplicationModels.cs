using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Groups multiple uploads that represent the *same logical scan* even when the
/// files differ (different format: PDF/CSV/.nessus, different export time, different
/// filename). A ScanGroup is the unit of deduplication and concurrency control.
///
/// Identity of a group is established by ScanKey below. When User A uploads Report A
/// and User B uploads the same scan as a PDF five minutes later, both uploads attach
/// to the same ScanGroup. Only one ingestion process runs per group at a time
/// (see ScanIngestionLock). Additional uploads are merged into the existing group.
/// </summary>
public class ScanGroup
{
    public int Id { get; set; }

    /// <summary>
    /// Deterministic key identifying the logical scan. Computed as (in priority order):
    ///   1. Nessus scan UUID (most reliable, survives re-export)
    ///   2. Otherwise composite: SHA256(ScannerName|PolicyId|ScanStartTime|ScanEndTime)
    /// This is the value used to detect "same scan, different file" (Scenario 4).
    /// </summary>
    public string ScanKey { get; set; } = default!;

    public string? NessusScanUuid { get; set; }
    public string? ScannerName { get; set; }
    public string? PolicyName { get; set; }
    public string? PolicyId { get; set; }

    public DateTime? ScanStart { get; set; }
    public DateTime? ScanEnd { get; set; }

    /// <summary>
    /// SourceType / cycle label captured from the first upload (Monthly, Quarterly, etc.)
    /// </summary>
    public string SourceType { get; set; } = "Monthly";
    public string? ScanCycleLabel { get; set; }

    /// <summary>Concurrency state of the group: Idle, Processing, Queued.</summary>
    public string IngestState { get; set; } = "Idle";

    public int TotalUploads { get; set; }
    public int TotalFindings { get; set; }
    public int TotalInstances { get; set; }
    public int TotalHosts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ScanUpload> Uploads { get; set; } = new List<ScanUpload>();
}

/// <summary>
/// Extracted, normalized Nessus scan metadata. One per ScanUpload. This is what we
/// use to compute the ScanGroup.ScanKey and to compare scans across formats
/// (Scenario 3 &amp; 4).
/// </summary>
public class ScanMetadata
{
    public int Id { get; set; }
    public int ScanUploadId { get; set; }
    public ScanUpload? ScanUpload { get; set; }

    public string? NessusScanUuid { get; set; }
    public string? ScannerName { get; set; }
    public string? PolicyName { get; set; }
    public string? PolicyId { get; set; }
    public DateTime? ScanStart { get; set; }
    public DateTime? ScanEnd { get; set; }
    public string? ScanTarget { get; set; }
    public string? Preference { get; set; }
}

/// <summary>
/// Per-upload deduplication decision log. Records exactly what happened to each
/// upload relative to the existing database and the owning ScanGroup
/// (Scenario 3,5,6,7,9,12,13).
/// </summary>
public class IngestionAudit
{
    public int Id { get; set; }
    public int ScanUploadId { get; set; }
    public ScanUpload? ScanUpload { get; set; }

    public int? ScanGroupId { get; set; }
    public ScanGroup? ScanGroup { get; set; }

    public int? PerformedByUserId { get; set; }
    public UserAccount? PerformedBy { get; set; }

    /// <summary>
    /// Outcome category: Ingested, Merged, Duplicate, Partial, Rejected, Queued.
    /// </summary>
    public string Outcome { get; set; } = default!;

    /// <summary>
    /// Duplicate status relative to file/scan levels (Scenario 3,4).
    /// ByteDuplicate / SameScan / Unique.
    /// </summary>
    public string DuplicateStatus { get; set; } = "Unique";

    public int NewFindings { get; set; }
    public int ExistingFindings { get; set; }
    public int ReopenedFindings { get; set; }
    public int RemediatedFindings { get; set; }
    public int RejectedFindings { get; set; }

    public long ProcessingMs { get; set; }
    public string? Reason { get; set; }
    public string? ProcessingLog { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Record-level deduplication decision for a single vulnerability instance during an
/// upload. Enables full drill-down of exactly which findings were new vs duplicate
/// vs reopened (Scenario 5,6,9,10).
/// </summary>
public class DeduplicationLog
{
    public int Id { get; set; }
    public int ScanUploadId { get; set; }
    public ScanUpload? ScanUpload { get; set; }

    /// <summary>
    /// Composite vulnerability identity used for matching:
    /// PluginId|AssetHostId|Port|Protocol  (normalized, lower-cased).
    /// </summary>
    public string VulnerabilityKey { get; set; } = default!;

    public int? VulnerabilityInstanceId { get; set; }
    public VulnerabilityInstance? VulnerabilityInstance { get; set; }

    public int PluginId { get; set; }
    public string? HostName { get; set; }
    public string? IpAddress { get; set; }
    public string? Cve { get; set; }
    public int? Port { get; set; }
    public string? Protocol { get; set; }

    /// <summary>
    /// Decision: New, Duplicate, Reopened, Updated, Remediated.
    /// </summary>
    public string Decision { get; set; } = default!;

    public int? MatchedExistingInstanceId { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Optimistic, database-backed concurrency lock for a ScanGroup. Used to guarantee
/// that only ONE ingestion process runs per logical scan at any time, even across
/// multiple web-server instances (Scenario 1,2,11). The controller acquires the lock
/// with a LeaseUntil timestamp; latecomers see state=Processing and are queued,
/// notified, or rejected per policy.
///
/// For a single-instance deployment a complementary in-process SemaphoreSlim is also
/// used (see ScanIngestionService) to avoid DB round-trips on the hot path.
/// </summary>
public class ScanIngestionLock
{
    public int Id { get; set; }

    /// <summary>ScanGroup.Id that this lock guards.</summary>
    public int ScanGroupId { get; set; }
    public ScanGroup? ScanGroup { get; set; }

    /// <summary>Owned by the ScanUpload currently processing, for "View existing upload".</summary>
    public int? OwnerUploadId { get; set; }
    public ScanUpload? OwnerUpload { get; set; }

    public string State { get; set; } = "Idle"; // Idle, Processing, Queued

    /// <summary>UTC timestamp until which the lease is valid. Expired leases are stealable.</summary>
    public DateTime LeaseUntil { get; set; }

    public int? LockedByUserId { get; set; }
    public UserAccount? LockedBy { get; set; }

    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}
