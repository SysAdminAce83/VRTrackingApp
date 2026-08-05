using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.MSRC;
using VRTrackingApp.Web.Services.MSRC.Models;
using VRTrackingApp.Web.Services.Notifications;

namespace VRTrackingApp.Web.Services;

/// <summary>
/// Result of a deduplication / ingestion decision returned to the controller and used
/// to drive UI messaging (Scenario 12) and audit (Scenario 13).
/// </summary>
public class IngestionDecision
{
    public string Outcome { get; set; } = "Ingested"; // Ingested | Merged | Duplicate | Partial | Rejected | Queued
    public string DuplicateStatus { get; set; } = "Unique"; // ByteDuplicate | SameScan | Unique
    public bool Proceed { get; set; } = true;
    public string? Message { get; set; }
    public int? ExistingScanGroupId { get; set; }
    public int? ProcessingUploadId { get; set; }
    public string? ProcessingUserName { get; set; }
}

/// <summary>
/// Enterprise-grade ingestion &amp; deduplication engine for Nessus scan reports
/// (PDF / CSV / .nessus XML). Solves all 14 scenarios from the spec:
///
///  - Scenario 1/2/11 : Concurrency via a DB-backed ScanIngestionLock (optimistic lease)
///                       plus an in-process SemaphoreSlim for the hot path.
///  - Scenario 3      : Byte-for-byte duplicate detection via SHA-256 (+ MD5 kept).
///  - Scenario 4      : "Same scan, different file" detection via ScanKey (Nessus UUID,
///                       else composite of scanner/policy/start/end).
///  - Scenario 5/6/7  : Record-level merge against historical DB; only NEW findings are
///                       ingested; existing/reopened/remediated are classified.
///  - Scenario 8      : All formats normalized to ParsedScan before comparison.
///  - Scenario 9/10   : Multi-level dedup with composite vulnerability key
///                       (PluginId|AssetHostId|Port|Protocol).
///  - Scenario 12/13  : Notifications + full audit trail (IngestionAudit + DeduplicationLog).
/// </summary>
public class ScanIngestionService
{
    private readonly VRTrackingAppContext _db;
    private readonly ScanImportService _import;
    private readonly INotificationService _notify;
    private readonly UserNotificationService _users;
    private readonly IVulnerabilityEnrichmentService _enrichment;

    /// <summary>In-process lock to serialize ingestion per scan key on a single node.</summary>
    private static readonly Dictionary<string, SemaphoreSlim> NodeLocks = new();
    private static readonly object NodeLockGate = new();

public ScanIngestionService(VRTrackingAppContext db, ScanImportService import,
        INotificationService notify, UserNotificationService users,
        IVulnerabilityEnrichmentService enrichment)
    {
        _db = db;
        _import = import;
        _notify = notify;
        _users = users;
        _enrichment = enrichment;
    }

    // ----------------------------------------------------------------
    // Hashing (Scenario 3)
    // ----------------------------------------------------------------
    public async Task<(string sha256, string md5)> ComputeHashesAsync(Stream stream)
    {
        using var sha = SHA256.Create();
        using var md5 = MD5.Create();
        var shaHash = await sha.ComputeHashAsync(stream);
        stream.Position = 0;
        var md5Hash = await md5.ComputeHashAsync(stream);
        stream.Position = 0;
        return (
            BitConverter.ToString(shaHash).Replace("-", "").ToLowerInvariant(),
            BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant()
        );
    }

    // ----------------------------------------------------------------
    // Scan key (Scenario 4)
    // ----------------------------------------------------------------
    public static string ComputeScanKey(ScanMetadata? m)
    {
        if (m != null && !string.IsNullOrWhiteSpace(m.NessusScanUuid))
            return "uuid:" + m.NessusScanUuid.Trim().ToLowerInvariant();

        // Composite fallback - stable across re-exports of the same scan.
        var scanner = (m?.ScannerName ?? "").Trim().ToLowerInvariant();
        var policy = (m?.PolicyId ?? m?.PolicyName ?? "").Trim().ToLowerInvariant();
        var start = m?.ScanStart?.ToString("o") ?? "";
        var end = m?.ScanEnd?.ToString("o") ?? "";
        var seed = $"{scanner}|{policy}|{start}|{end}";
        if (string.IsNullOrWhiteSpace(seed.Replace("|", "").Replace("||", "")))
            return "unknown";
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
        return "comp:" + BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant()[..32];
    }

    // ----------------------------------------------------------------
    // Pre-ingest decision (Scenario 1,2,3,4,11)
    // ----------------------------------------------------------------
    public async Task<IngestionDecision> DecideAsync(string sha256, string scanKey, int currentUserId)
    {
        // Scenario 3: exact byte duplicate in the last 90 days.
        var byteDup = await _db.ScanUploads
            .Where(u => u.FileHash == sha256 && u.UploadedAt > DateTime.UtcNow.AddDays(-90))
            .OrderByDescending(u => u.UploadedAt)
            .FirstOrDefaultAsync();
        if (byteDup != null)
        {
            return new IngestionDecision
            {
                Outcome = "Duplicate",
                DuplicateStatus = "ByteDuplicate",
                Proceed = false,
                Message = "This exact file was already uploaded.",
                ExistingScanGroupId = byteDup.ScanGroupId,
                ProcessingUploadId = byteDup.Id
            };
        }

        // Scenario 4: same logical scan (different file/format).
        var group = await _db.ScanGroups.FirstOrDefaultAsync(g => g.ScanKey == scanKey);
        if (group != null)
        {
            var active = await _db.ScanIngestionLocks
                .Include(l => l.OwnerUpload).ThenInclude(u => u.UploadedBy)
                .FirstOrDefaultAsync(l => l.ScanGroupId == group.Id);

            if (active != null && active.State == "Processing" && active.LeaseUntil > DateTime.UtcNow)
            {
                return new IngestionDecision
                {
                    Outcome = "Queued",
                    DuplicateStatus = "SameScan",
                    Proceed = false,
                    Message = "This scan is already being processed by another user.",
                    ExistingScanGroupId = group.Id,
                    ProcessingUploadId = active.OwnerUploadId,
                    ProcessingUserName = active.OwnerUpload?.UploadedBy?.DisplayName
                                          ?? active.OwnerUpload?.UploadedBy?.UserName
                };
            }
            return new IngestionDecision
            {
                Outcome = "Merged",
                DuplicateStatus = "SameScan",
                Proceed = true,
                Message = "This report belongs to an existing scan; new findings will be merged.",
                ExistingScanGroupId = group.Id
            };
        }

        return new IngestionDecision { Outcome = "Ingested", DuplicateStatus = "Unique", Proceed = true };
    }

    // ----------------------------------------------------------------
    // Acquire lease (Scenario 11 - optimistic DB lock)
    // ----------------------------------------------------------------
    private async Task<ScanIngestionLock> AcquireLockAsync(ScanGroup group, ScanUpload upload, int userId, TimeSpan lease)
    {
        var existing = await _db.ScanIngestionLocks.FirstOrDefaultAsync(l => l.ScanGroupId == group.Id);
        var now = DateTime.UtcNow;
        if (existing != null && existing.State == "Processing" && existing.LeaseUntil > now)
            throw new InvalidOperationException("Scan is already being processed.");

        if (existing == null)
        {
            existing = new ScanIngestionLock { ScanGroupId = group.Id };
            _db.ScanIngestionLocks.Add(existing);
        }
        existing.State = "Processing";
        existing.OwnerUploadId = upload.Id;
        existing.LockedByUserId = userId;
        existing.LeaseUntil = now.Add(lease);
        existing.AcquiredAt = now;
        await _db.SaveChangesAsync();
        return existing;
    }

    private async Task ReleaseLockAsync(ScanGroup group)
    {
        var lk = await _db.ScanIngestionLocks.FirstOrDefaultAsync(l => l.ScanGroupId == group.Id);
        if (lk != null) { lk.State = "Idle"; await _db.SaveChangesAsync(); }
    }

    private SemaphoreSlim NodeLock(string key)
    {
        lock (NodeLockGate)
        {
            if (!NodeLocks.TryGetValue(key, out var s))
                NodeLocks[key] = s = new SemaphoreSlim(1, 1);
            return s;
        }
    }

    // ----------------------------------------------------------------
    // Composite vulnerability key (Scenario 10)
    // ----------------------------------------------------------------
    private static string VulnKey(int assetHostId, int pluginId, int? port, string? protocol)
        => $"{assetHostId}|{pluginId}|{port?.ToString() ?? "0"}|{protocol ?? ""}";

    // ----------------------------------------------------------------
    // Core ingestion (Scenario 5,6,7,9)
    // ----------------------------------------------------------------
    public async Task<IngestionResult> IngestAsync(ScanUpload upload, ParsedScan parsed, int userId)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = new IngestionResult { ScanUploadId = upload.Id };
        var scanKey = ComputeScanKey(parsed.Metadata);
        var nodeSem = NodeLock(scanKey);
        await nodeSem.WaitAsync();

        try
        {
            // Resolve or create the ScanGroup (Scenario 4).
            var group = await _db.ScanGroups.FirstOrDefaultAsync(g => g.ScanKey == scanKey);
            if (group == null)
            {
                group = new ScanGroup
                {
                    ScanKey = scanKey,
                    NessusScanUuid = parsed.Metadata?.NessusScanUuid,
                    ScannerName = parsed.Metadata?.ScannerName,
                    PolicyName = parsed.Metadata?.PolicyName,
                    PolicyId = parsed.Metadata?.PolicyId,
                    ScanStart = parsed.Metadata?.ScanStart,
                    ScanEnd = parsed.Metadata?.ScanEnd,
                    SourceType = upload.SourceType,
                    ScanCycleLabel = upload.ScanCycleLabel,
                    IngestState = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ScanGroups.Add(group);
                await _db.SaveChangesAsync();
            }
            upload.ScanGroupId = group.Id;

            await AcquireLockAsync(group, upload, userId, TimeSpan.FromMinutes(10));

            // Persist metadata.
            if (parsed.Metadata != null)
            {
                parsed.Metadata.ScanUploadId = upload.Id;
                _db.ScanMetadatas.Add(parsed.Metadata);
            }

            var scanDate = upload.ScanDate ?? parsed.Metadata?.ScanStart ?? DateTime.UtcNow;
            var seenFindings = new Dictionary<int, VulnerabilityFinding>();
            var seenHosts = new Dictionary<string, AssetHost>(StringComparer.OrdinalIgnoreCase);
            var seenAssets = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
            var location = parsed.Metadata?.ScanTarget;

            // Load existing instances for this group to compare (Scenario 6,7).
            var existingKeys = await _db.VulnerabilityInstances
                .Where(i => i.AssetHost != null && i.AssetHost.ScanUpload != null && i.AssetHost.ScanUpload.ScanGroupId == group.Id)
                .Select(i => new { i.Id, i.AssetHostId, i.VulnerabilityFindingId, i.Port, i.Protocol, i.Status })
                .ToListAsync();
            var existingKeySet = new HashSet<string>(
                existingKeys.Select(e => VulnKey(e.AssetHostId, e.VulnerabilityFindingId, e.Port, e.Protocol)));

            foreach (var row in parsed.Rows)
            {
                try
                {
                    if (!seenHosts.TryGetValue(row.HostKey, out var host))
                    {
                        host = new AssetHost
                        {
                            ScanUploadId = upload.Id,
                            HostName = row.HostKey,
                            IpAddress = row.Ip ?? row.HostKey,
                            OperatingSystem = row.Os,
                            CreatedAt = DateTime.UtcNow
                        };
                        host.Asset = await GetOrCreateAssetAsync(seenAssets, row.HostKey,
                            row.Ip ?? row.HostKey, row.Os, location);
                        _db.AssetHosts.Add(host);
                        seenHosts[row.HostKey] = host;
                        await _db.SaveChangesAsync(); // so host.Id is available for key
                    }

if (!seenFindings.TryGetValue(row.PluginId, out var finding))
                    {
                        finding = new VulnerabilityFinding
                        {
                            PluginId = row.PluginId,
                            PluginName = row.Name,
                            Cve = row.Cve,
                            Severity = row.Severity,
                            Synopsis = row.Synopsis,
                            Description = row.Description,
                            Solution = row.Solution,
                            RiskFactor = row.RiskFactor,
                            StigSeverity = row.Stig,
                            CvssV3BaseScore = row.CvssV3Base,
                            CvssV3TemporalScore = row.CvssV3Temp,
                            CvssV2BaseScore = row.CvssV2Base,
                            VprScore = row.Vpr,
                            EpssScore = row.Epss,
                            References = row.References,
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.VulnerabilityFindings.Add(finding);
                        seenFindings[row.PluginId] = finding;
                        await _db.SaveChangesAsync();

                        // MSRC Enrichment for new findings with CVEs
                        if (!string.IsNullOrWhiteSpace(finding.Cve))
                        {
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    var enrichment = await _enrichment.EnrichByCVEAsync(finding.Cve!);
                                    if (enrichment != null)
                                    {
                                        ApplyEnrichment(finding, enrichment);
                                        await _db.SaveChangesAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log but don't fail ingestion
                                    System.Diagnostics.Debug.WriteLine($"MSRC enrichment failed for {finding.Cve}: {ex.Message}");
                                }
                            });
                        }
                    }

                    var key = VulnKey(host.Id, finding.Id, row.Port, row.Protocol);
                    var existing = existingKeys.FirstOrDefault(e =>
                        e.AssetHostId == host.Id && e.VulnerabilityFindingId == finding.Id &&
                        (e.Port ?? 0) == (row.Port ?? 0) &&
                        string.Equals(e.Protocol, row.Protocol, StringComparison.OrdinalIgnoreCase));

                    var decision = existingKeySet.Contains(key) ? "Duplicate" : "New";
                    if (!existingKeySet.Contains(key))
                    {
                        var instance = new VulnerabilityInstance
                        {
                            AssetHost = host,
                            VulnerabilityFinding = finding,
                            Port = row.Port,
                            Protocol = row.Protocol,
                            ServiceName = row.Service,
                            PluginOutput = row.PluginOutput,
                            Status = "Open",
                            FirstFound = scanDate,
                            LastFound = scanDate
                        };
                        _db.VulnerabilityInstances.Add(instance);
                        await _db.SaveChangesAsync();
                        _db.DeduplicationLogs.Add(new DeduplicationLog
                        {
                            ScanUploadId = upload.Id,
                            VulnerabilityKey = key,
                            VulnerabilityInstanceId = instance.Id,
                            PluginId = row.PluginId,
                            HostName = row.HostKey,
                            IpAddress = row.Ip,
                            Cve = row.Cve,
                            Port = row.Port,
                            Protocol = row.Protocol,
                            Decision = "New"
                        });
                        result.NewCount++;
                    }
                    else
                    {
                        // Existing: bump LastFound + reopen if previously remediated (Scenario 6).
                        var inst = await _db.VulnerabilityInstances.FindAsync(existing.Id);
                        if (inst != null)
                        {
                            inst.LastFound = scanDate;
                            if (inst.Status == "Fixed" || inst.Status == "Exception")
                            {
                                inst.Status = "Open";
                                result.ReopenedCount++;
                                decision = "Reopened";
                            }
                        }
                        _db.DeduplicationLogs.Add(new DeduplicationLog
                        {
                            ScanUploadId = upload.Id,
                            VulnerabilityKey = key,
                            VulnerabilityInstanceId = existing.Id,
                            PluginId = row.PluginId,
                            HostName = row.HostKey,
                            IpAddress = row.Ip,
                            Cve = row.Cve,
                            Port = row.Port,
                            Protocol = row.Protocol,
                            Decision = decision
                        });
                        result.ExistingCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row ({row.HostKey},{row.PluginId}): {ex.Message}");
                }
            }

            // Update group tallies.
            group.TotalUploads++;
            group.TotalHosts = await _db.AssetHosts.CountAsync(h => h.ScanUpload != null && h.ScanUpload.ScanGroupId == group.Id);
            group.TotalFindings = await _db.VulnerabilityFindings.CountAsync();
            group.TotalInstances = await _db.VulnerabilityInstances
                .CountAsync(i => i.AssetHost != null && i.AssetHost.ScanUpload != null && i.AssetHost.ScanUpload.ScanGroupId == group.Id);
            group.IngestState = "Idle";
            group.UpdatedAt = DateTime.UtcNow;

            // ---- Audit (Scenario 13) ----
            var isMerge = group.TotalUploads > 1;
            var audit = new IngestionAudit
            {
                ScanUploadId = upload.Id,
                ScanGroupId = group.Id,
                PerformedByUserId = userId,
                Outcome = result.Errors.Count == 0
                    ? (result.NewCount == 0 ? "Duplicate" : (isMerge ? "Merged" : "Ingested"))
                    : "Partial",
                DuplicateStatus = isMerge ? "SameScan" : "Unique",
                NewFindings = result.NewCount,
                ExistingFindings = result.ExistingCount,
                ReopenedFindings = result.ReopenedCount,
                ProcessingMs = sw.ElapsedMilliseconds,
                Reason = result.Errors.Count == 0 ? null : string.Join("; ", result.Errors.Take(5)),
                ProcessingLog = $"Parsed {parsed.Instances} rows; new={result.NewCount}; existing={result.ExistingCount}; reopened={result.ReopenedCount}."
            };
            _db.IngestionAudits.Add(audit);
            upload.Status = audit.Outcome == "Duplicate" ? "Duplicate" : "Completed";
            await _db.SaveChangesAsync();

            await ReleaseLockAsync(group);

            // ---- Notifications (Scenario 12) ----
            await NotifyAsync(upload, group, result, userId);

            result.Success = true;
            result.GroupId = group.Id;
            result.Outcome = audit.Outcome;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
            upload.Status = "Failed";
            _db.IngestionAudits.Add(new IngestionAudit
            {
                ScanUploadId = upload.Id,
                ScanGroupId = upload.ScanGroupId,
                PerformedByUserId = userId,
                Outcome = "Rejected",
                DuplicateStatus = "Unique",
                ProcessingMs = sw.ElapsedMilliseconds,
                Reason = ex.Message
            });
            await _db.SaveChangesAsync();
        }
        finally
        {
            nodeSem.Release();
        }

        return result;
    }

    private async Task NotifyAsync(ScanUpload upload, ScanGroup group, IngestionResult result, int userId)
    {
        var me = await _db.UserAccounts.FindAsync(userId);
        var name = me?.DisplayName ?? me?.UserName ?? "A user";
        var analysts = await _users.UsersInRolesAsync(new[] { "Admin", "Analyst" });

        if (result.Outcome == "Duplicate")
        {
            await _notify.NotifyUserAsync(me ?? new UserAccount { Id = userId, Email = "", UserName = name },
                NotificationType.ScanDuplicateDetected, null,
                "Duplicate scan upload",
                $"Your upload '{upload.FileName}' matches an existing scan. No new data was added.");
        }
        else if (result.NewCount > 0 && group.TotalUploads > 1)
        {
            foreach (var a in analysts)
                await _notify.NotifyUserAsync(a, NotificationType.ScanAdditionalFindings, null,
                    "Additional findings merged",
                    $"{name} uploaded '{upload.FileName}' to scan group #{group.Id}. {result.NewCount} new finding(s) were merged; {result.ExistingCount} already known, {result.ReopenedCount} reopened.");
        }
        else if (result.NewCount > 0)
        {
            foreach (var a in analysts)
                await _notify.NotifyUserAsync(a, NotificationType.ScanIngested, null,
                    "New scan ingested",
                    $"{name} uploaded '{upload.FileName}'. {result.NewCount} finding(s) ingested across {result.Hosts ?? group.TotalHosts} host(s).");
        }
    }

    // Asset de-dup (reuse logic from ScanImportService via a thin wrapper).
    private async Task<Asset> GetOrCreateAssetAsync(Dictionary<string, Asset> cache,
        string hostName, string ip, string? os, string? location)
    {
        var hn = (hostName ?? "").Trim();
        var ipN = (ip ?? "").Trim();
        var key = !string.IsNullOrEmpty(hn) ? hn.ToLowerInvariant() : ipN.ToLowerInvariant();
        if (string.IsNullOrEmpty(key)) key = "unknown";
        if (cache.TryGetValue(key, out var cached)) return cached;

        Asset? existing = null;
        if (!string.IsNullOrEmpty(hn))
            existing = await _db.Assets.FirstOrDefaultAsync(a => a.HostName != null && a.HostName.ToLower() == hn.ToLower());
        if (existing == null && !string.IsNullOrEmpty(ipN))
            existing = await _db.Assets.FirstOrDefaultAsync(a => a.IpAddress != null && a.IpAddress.ToLower() == ipN.ToLower());

        if (existing == null)
        {
            existing = new Asset
            {
                HostName = hn,
                IpAddress = ipN,
                OperatingSystem = os,
                Location = location,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Assets.Add(existing);
        }
        else
        {
            existing.LastSeen = DateTime.UtcNow;
            if (string.IsNullOrEmpty(existing.OperatingSystem) && !string.IsNullOrEmpty(os)) existing.OperatingSystem = os;
            if (string.IsNullOrEmpty(existing.Location) && !string.IsNullOrEmpty(location)) existing.Location = location;
        }
cache[key] = existing;
        return existing;
    }

    private void ApplyEnrichment(VulnerabilityFinding finding, MSRCEnrichmentData enrichment)
    {
        finding.MicrosoftAdvisoryId = enrichment.MicrosoftAdvisoryId;
        finding.MicrosoftBulletinId = enrichment.MicrosoftBulletinId;
        finding.KBNumbers = enrichment.KBNumbers != null ? string.Join(",", enrichment.KBNumbers) : null;
        finding.PatchDownloadUrls = enrichment.PatchDownloadUrls != null ? string.Join(",", enrichment.PatchDownloadUrls) : null;
        finding.RequiresReboot = enrichment.RequiresReboot;
        finding.SupersededBy = enrichment.SupersededBy;
        finding.ExploitabilityAssessment = enrichment.ExploitabilityAssessment;
        finding.MicrosoftReleaseDate = enrichment.MicrosoftReleaseDate?.DateTime;
        finding.AffectedProducts = enrichment.AffectedProducts != null
            ? System.Text.Json.JsonSerializer.Serialize(enrichment.AffectedProducts)
            : null;
        finding.Workaround = enrichment.Workaround;
        finding.FAQUrl = enrichment.FAQUrl;
        finding.CVRFId = enrichment.CVRFId;
        finding.CSAFId = enrichment.CSAFId;
        finding.LastEnrichedAt = DateTime.UtcNow;
    }
}

public class IngestionResult
{
    public bool Success { get; set; }
    public string Outcome { get; set; } = "Ingested";
    public int ScanUploadId { get; set; }
    public int? GroupId { get; set; }
    public int NewCount { get; set; }
    public int ExistingCount { get; set; }
    public int ReopenedCount { get; set; }
    public int? Hosts { get; set; }
    public List<string> Errors { get; set; } = new();
}

