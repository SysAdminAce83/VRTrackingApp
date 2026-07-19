# Enterprise Nessus Ingestion & Deduplication Workflow

This document describes the design and implementation of the enterprise-grade scan
ingestion & deduplication engine adopted into **VRTrackingApp** (VR Remediation Console).
It is a direct implementation of the 14-scenario specification for multi-engineer,
concurrent Nessus report uploads.

---

## 1. High-Level Architecture

```
                 +-------------------+        +----------------------------+
  PDF/CSV/       |  UploadController  |        |   ScanIngestionService     |
  .nessus  ----> |  (Validate→Preview |------->|  (engine: lock+dedup+merge) |
  upload         |   → Confirm)       |        |                            |
                 +-------------------+        +-------------+--------------+
                                                         |  parse
                                                         v
                                          +--------------+--------------+
                                          |  Format parsers -> ParsedScan |
                                          |  CSV | PDF | Nessus XML       |
                                          +--------------+--------------+
                                                         | normalized model
                                                         v
                 +-------------------+        +-------------+--------------+
                 |  ScanGroup (logical| <----- |  Dedup engine (levels 1-4) |
                 |   scan identity)   |        |  + DB merge + reopen logic |
                 +-------------------+        +-------------+--------------+
                                                         |
                                    +--------------------+---------------------+
                                    v                    v                     v
                            ScanUpload            VulnerabilityInstance    IngestionAudit
                            ScanMetadata          VulnerabilityFinding     DeduplicationLog
                            ScanIngestionLock     AssetHost / Asset        Notification
```

**Key principle:** every upload is first normalized into a single common model
(`ParsedScan`) regardless of source format, so the deduplication and merge logic
only ever operates on one shape (Scenario 8).

---

## 2. Normalized Data Model (Scenario 8)

All three supported formats are parsed into `ParsedScan`:

| Field | CSV | PDF | .nessus XML |
|-------|-----|-----|-------------|
| Host / IP / OS | yes | heuristic | authoritative (`HostProperties`) |
| Plugin ID | yes | heuristic | `ReportItem/@pluginID` |
| Severity | yes | heuristic | `risk_factor` |
| CVE | yes | regex | `cve` |
| Port / Protocol | yes | heuristic | `ReportItem/@port|protocol` |
| **Scan UUID** | — | — | `ServerPreferences/report_uuid` |
| Scanner / Policy | — | — | `scanner_name` / `policy_name` / `policy_id` |
| Scan start/end | user-supplied | — | `scan_start` / `scan_end` |

`NessusXmlParser` is the richest source because the native `.nessus` carries the
authoritative scan metadata needed for the most reliable identity (Scenario 3/4).

---

## 3. Multi-Level Deduplication Strategy (Scenario 9, 10)

Detection runs top-down; the first match wins.

### Level 1 — File level (byte-for-byte) — Scenario 3
`SHA-256` of the raw bytes (MD5 kept for legacy interop). If an identical hash was
uploaded in the last **90 days**, the upload is rejected as a `ByteDuplicate`.
Most reliable for "same exact file" (Scenario 3).

### Level 2 — Scan level (same logical scan, different file) — Scenario 4
A `ScanKey` identifies the logical scan:
- **Primary:** `uuid:<NessusScanUuid>` (survives re-export / format change).
- **Fallback:** `comp:<SHA256(scanner|policy|scanStart|scanEnd)>` (stable across
  re-exports that preserve metadata).
All uploads sharing a `ScanKey` attach to one `ScanGroup`.

### Level 3 — Asset level
`AssetHost` is matched by `HostName` then `IP` and linked to a canonical `Asset`
(record-level de-dup of hosts across uploads).

### Level 4 — Vulnerability (record) level — Scenario 5, 6, 7
Composite key per instance:
```
VulnerabilityKey = AssetHostId | PluginId | Port | Protocol
```
On ingest, each parsed row is compared against existing instances **within the same
ScanGroup**. `New` rows are inserted; existing rows are flagged `Duplicate` and have
`LastFound` bumped; previously `Fixed`/`Exception` rows are flipped back to `Open`
(`Reopened`).

### Recommended unique vulnerability identity (Scenario 10)
For *cross-scan* matching we use:
```
PluginId + (HostName|IP) + Port + Protocol + CVE
```
`PluginId` is the Nessus-native stable identifier; host identity uses name OR IP;
port/protocol disambiguate multiple services; CVE links the same weakness across
plugin versions.

---

## 4. Concurrency Control (Scenario 1, 2, 11)

Two complementary locks guarantee **exactly one ingestion process per logical scan**:

1. **In-process `SemaphoreSlim`** keyed by `ScanKey` — serializes ingestion on a
   single web node without a DB round-trip on the hot path.
2. **Database-backed `ScanIngestionLock`** (optimistic lease) — survives multiple
   web nodes. Acquired with a `LeaseUntil` timestamp (10 min). A second upload that
   arrives while `State == "Processing"` and the lease is valid receives a
   `Queued` decision with the name of the user currently processing, plus a link to
   the existing upload. Expired leases are stealable (crash recovery).

This is **optimistic + lease-based** locking — the recommended enterprise pattern
for a single-writer-per-scan workload. (For larger estates, swap the DB lock for a
Redis / ZooKeeper distributed lock or push ingestion onto a queue
— see Section 10.)

---

## 5. Workflow (Scenario 14)

```
Upload → Validate(IsAllowedFile, size≤100MB)
   → Store with random server name (never trust client filename)
   → Compute SHA-256 + MD5
   → Parse to ParsedScan (normalized)
   → DecideAsync():
        • ByteDuplicate  → reject (Scenario 3)
        • Processing+lease valid → Queued, notify (Scenario 2)
        • Same ScanKey   → Merged (Scenario 4)
        • else           → Ingested
   → Confirm → IngestAsync():
        • Acquire node semaphore + DB lock (Scenario 11)
        • Resolve/Create ScanGroup (Scenario 4)
        • Persist ScanMetadata
        • For each row: host/asset/finding dedup, insert new, bump existing, reopen (Scenario 5,6,7)
        • Update group tallies
        • Write IngestionAudit + DeduplicationLog rows (Scenario 13)
        • Release lock
        • Notify (Scenario 12)
        • Redirect to Scans/Details
```

Failure handling: any exception writes an `IngestionAudit` with `Outcome=Rejected`
and `Reason`, releases the lock, and marks `ScanUpload.Status=Failed`. The single
`UploadAuditTrail` is retained for legacy history.

---

## 6. Notifications (Scenario 12)

Sample UI messages produced by `ScanIngestionService.NotifyAsync`:

| Event | Title | Message |
|-------|-------|---------|
| Duplicate | "Duplicate scan upload" | "Your upload 'X' matches an existing scan. No new data was added." |
| Already processing | (Queued banner) | "This scan is already being processed by User A. View existing upload." |
| Additional findings | "Additional findings merged" | "{user} uploaded 'X' to scan group #N. 10 new finding(s) merged; 100 already known, 2 reopened." |
| New scan | "New scan ingested" | "{user} uploaded 'X'. 100 findings ingested across 12 hosts." |

Notification types: `ScanIngested`, `ScanDuplicateDetected`, `ScanAlreadyProcessing`,
`ScanAdditionalFindings`, `ScanRejected`, `ScanMerged`. In-app (`Notification` table)
+ optional email (best-effort SMTP channel).

---

## 7. Audit Trail Schema (Scenario 13)

`IngestionAudit` (one per upload):
`ScanUploadId, ScanGroupId, PerformedByUserId, Outcome, DuplicateStatus,
NewFindings, ExistingFindings, ReopenedFindings, RemediatedFindings, RejectedFindings,
ProcessingMs, Reason, ProcessingLog, PerformedAt`.

`DeduplicationLog` (one per vulnerability instance):
`ScanUploadId, VulnerabilityKey, VulnerabilityInstanceId, PluginId, HostName,
IpAddress, Cve, Port, Protocol, Decision (New|Duplicate|Reopened), MatchedExistingInstanceId`.

Both are viewable at `Upload/History/{id}` and summarized at `Upload/Group/{id}`.

---

## 8. SQL / Indexing Strategy (fast duplicate detection)

```
-- File-level (Scenario 3): exact byte match
CREATE UNIQUE INDEX IX_ScanUploads_FileHash ...;          -- queried with WHERE FileHash=@h

-- Scan-level (Scenario 4): logical scan identity
CREATE UNIQUE INDEX IX_ScanGroups_ScanKey ...;            -- WHERE ScanKey=@k
CREATE INDEX IX_ScanGroups_NessusScanUuid ...;

-- Concurrency (Scenario 11)
CREATE UNIQUE INDEX IX_ScanIngestionLocks_ScanGroupId ...; -- one lock per group

-- Record-level merge (Scenario 7): avoid full scans
CREATE INDEX IX_VulnerabilityInstances_AssetHostId ...;
CREATE INDEX IX_DeduplicationLogs_VulnerabilityKey ...;
CREATE INDEX IX_DeduplicationLogs_(PluginId,HostName,Port,Protocol) ...;

-- Audit / dashboard
CREATE INDEX IX_IngestionAudits_ScanGroupId, _ScanUploadId, _PerformedAt;
```

Because the merge only loads instances **for the existing ScanGroup** (not the whole
history), a partial-overlap scan of 520 vs 500 (Scenario 7) compares at most ~500
rows — O(n) in the group size, not O(millions).

---

## 9. Handling Millions of Records (Scenario 6, 7)

- **Scope the comparison** to the owning `ScanGroup`; historical DB is only touched
  for `LastFound`/`Reopened` updates on matched keys.
- **Batched SaveChanges** per row keep memory bounded and transactions short.
- **Composite indexes** make the key lookup O(log n).
- `VulnerabilityFinding` is de-duplicated by `PluginId` within an upload (already in
  code) so the findings table does not balloon.
- For multi-year history correlation (reopened / previously remediated / same CVE
  different plugin), the `DeduplicationLog` + `FirstFound`/`LastFound` columns give a
  full lineage without re-scanning all rows.

---

## 10. Future Scalability

| Concern | Current | Recommended upgrade |
|---------|---------|---------------------|
| Lock | DB lease + in-proc semaphore | Redis RedLock / ZooKeeper for multi-node |
| Processing | Synchronous in request | Background queue (IHostedService / Azure Queue / RabbitMQ) — returns 202 + poll |
| Parsing | In-process | Serverless parser (Azure Function / Lambda) for 100 MB+ files |
| Storage | Local `Uploads/` | Blob storage (S3/Azure Blob) with SAS tokens |
| Notifications | Sync in request | Outbox pattern + message bus |
| History | Relational | Read replica / columnar store for analytics |

The engine is intentionally decoupled: `ScanIngestionService.IngestAsync` is the single
seam to move onto a queue worker.

---

## 11. Files Changed / Added

**Models** (`src/VRTrackingApp.Data/Models/`)
- `ScanUpload.cs` — added `FileHash`(64), `Md5Hash`, `FileSize`, `Format`, `ScanGroupId`, navigation to `ScanGroup`/`ScanMetadata`/`IngestionAudit`.
- `ScanDeduplicationModels.cs` — `ScanGroup`, `ScanMetadata`, `IngestionAudit`, `DeduplicationLog`, `ScanIngestionLock`.
- `ExceptionEnums.cs` — added `Scan*` notification types.

**Context**
- `VRTrackingAppContext.cs` — new `DbSet`s + indexes + relationships.

**Services** (`VRTrackingApp.Web/Services/`)
- `NessusXmlParser.cs` — `.nessus` XML parser + `ParsedScan` DTO + `ParsedHostFinding`.
- `ScanImportService.cs` — added `ParseCsvToModelAsync` / `ParsePdfToModelAsync` / `ParseTxtToModelAsync` (normalized model) + `.nessus`/`.xml` allow-list.
- `ScanIngestionService.cs` — the engine (hash, scan-key, decide, lock, merge, audit, notify).

**Controllers / Views**
- `UploadController.cs` — `Validate` (hash+parse+decide), `Confirm` (ingest), `Groups`, `Group`, `History`.
- `Views/Upload/{Preview,Groups,Group,History}.cshtml`, `Index.cshtml` (accept `.nessus`).
- `_Layout.cshtml` — "Scan Groups" nav link; `site.css` — dedup banner styles.

**Migration**
- `20260719053536_ScanDeduplication` (SQL Server mode). InMemory mode (default for the
  demo) requires no migration.
