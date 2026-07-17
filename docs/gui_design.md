# Vulnerability Remediation Console — GUI Design Document

> Companion to `architecture.md`, `database_schema.md`, `implementation_plan.md`, and `appdevpromt.md`.
> This document specifies the **presentation layer** (the `VRTrackingApp.Web` MVC application) as a
> workflow-driven remediation console for Nessus scan ingestion and tracking.

## 1. Design Goal

The application is a **remediation console**, not a simple upload-and-view portal. Users move through a
repeatable lifecycle:

```
Upload → Parse/Validate → Review (by host & vuln) → Assign Remediation Status
      → Track Exception / Fixed → Compare Scan Cycles → Report Progress
```

The GUI is built so analysts can **import once and review many times**, with one finding tracked across
multiple scan cycles while historical status is preserved.

## 2. Five-Layer Application Pattern

| # | Layer | Responsibility | Primary View(s) |
|---|-------|----------------|-----------------|
| 1 | **Upload Layer** | Accept Nessus CSV/PDF, validate, compute hash | `Upload` |
| 2 | **Parsing Layer** | Extract host + vulnerability records into normalized rows | `Upload` (parse status) |
| 3 | **Review Layer** | Inspect findings by host and by vulnerability | `Scans`, `Hosts`, `Vulnerabilities` |
| 4 | **Remediation Layer** | Mark each finding `Open` / `Fixed` / `Exception` | `Vulnerabilities/Details`, `Hosts/Details` |
| 5 | **Reporting Layer** | Progress over time, exceptions, export (CSV/PDF) | `Reports`, `Exceptions`, `Audit Log` |

## 3. Data Model Used by the GUI

The Web project references `src/VRTrackingApp.Data` (`VRTrackingApp.Data.Models`, context
`VRTrackingAppContext`). The model was extended to support the full vulnerability detail screen and
remediation workflow:

| Entity | Key Fields | Purpose |
|--------|-----------|---------|
| `ScanUpload` | FileName, FileHash, FileSize, Status, CycleLabel, ScanDate, SourceType (`Monthly`/`Patch Tuesday`/`Zero Day`/`Risk-based`), Notes, UploadedByUserId, UploadedAt | One imported report (a scan cycle). |
| `AssetHost` | ScanUploadId, HostName, IpAddress, OperatingSystem, CreatedAt | One scanned host. |
| `VulnerabilityFinding` | PluginId, PluginName, Cve, Severity, Synopsis, Description, Solution, RiskFactor, CvssV3Base/Temporal, CvssV2Base/Temporal, VprScore, EpssScore, StigSeverity, References | The normalized plugin-level definition (unique per PluginId). |
| `VulnerabilityInstance` | AssetHostId, VulnerabilityFindingId, Port, Protocol, ServiceName, PluginOutput, Status (`Open`/`Fixed`/`Exception`), FirstFound, LastFound, OwnerUserId, DueDate | One finding on one host (the unit of remediation). |
| `RemediationAction` | VulnerabilityInstanceId, Action, Status, AssignedToUserId (owner), DueDate, ExceptionExpiryDate, Comments, EvidenceFileName, CreatedAt | Change-history row for every status change. |
| `ExceptionRecord` | VulnerabilityInstanceId, Reason, ApprovedByUserId, ExpiresAt, CreatedAt | Active exception with approval + expiry. |
| `UploadAuditTrail` | ScanUploadId, Action, PerformedByUserId, PerformedAt | Audit of uploads/parsing. |
| `UserAccount` / `Role` | UserName, Email, DisplayName, IsActive, RoleId | Owners, approvers, auditors. |

> "One finding can exist across multiple scans" → a `VulnerabilityFinding` is matched by `PluginId` across
> cycles; each `VulnerabilityInstance` is the host+cycle occurrence that carries remediation state.

## 4. Screen Flow & Wireframes

### 4.1 Global Shell
```
┌───────────────────────────────────────────────────────────────────────────┐
│ Top Bar: [🔍 Global Search]   [Scan Cycle ▾] [Date ▾]        [🔔][Profile] │
├──────────┬────────────────────────────────────────────────────────────────┤
│ Left Nav │  Main Content Area                                              │
│ Dashboard│                                                                 │
│ Upload   │                                                                 │
│ Scans    │                                                                 │
│ Hosts    │                                                                 │
│ Vulns    │                                                                 │
│ Exc.     │                                                                 │
│ Reports  │                                                                 │
│ Audit    │                                                                 │
│ Admin    │                                                                 │
└──────────┴────────────────────────────────────────────────────────────────┘
```

### 4.2 Dashboard
```
KPI Cards: Total Hosts | Total Vulns | Open | Fixed | Exceptions | Critical/High
┌─────────────────────────────┬──────────────────────────┐
│ Remediation Trend (by cycle)│ Severity Distribution     │
│ (stacked bar / line, CSS)   │ (donut/bars, CSS)         │
└─────────────────────────────┴──────────────────────────┘
Recent Scans (table)        | Overdue / Expiring Items
```

### 4.3 Upload Scan Report
```
[ Drag & Drop zone ]   accepted: .csv / .pdf (validated server-side)
Scan Cycle Label: [______]   Scan Date: [date]   Source: [Monthly▾]
Notes: [_____________________________]
[ Upload & Parse ]  → parsing status: Validated ✓ / Records imported N / Errors
```
Server enforces: extension allow-list, size limit, content sniffing, SHA-256 hash, random storage name
(no trust of client filename/type).

### 4.4 Parsed Scan Review (Scans)
```
Header: scan name · date · file type · status
Left: Host list (hostname, ip, os)   Right: selected host vulnerability summary
Bottom: remediation actions + audit trail
```

### 4.5 Host Detail
```
Host identity + scan metadata
Vulnerabilities grouped by severity (Critical/High/Medium/Low)
Filters: [Open][Fixed][Exception]   Owner / Due
```

### 4.6 Vulnerability Detail (primary working screen)
```
Header: Vulnerability ID (PluginId) + Severity badge
Left column (raw): Description, Synopsis, Solution, Risk Factor,
  CVSS v3.0 Base/Temporal, CVSS v2.0 Base/Temporal, VPR, EPSS, STIG, References, Plugin Output (collapsible)
Right column (remediation):
  Status [Open/Fixed/Exception] · Owner · Due Date · Exception Expiry · Comments · Evidence attach
Bottom: Change history (RemediationAction rows) + Audit trail
```

### 4.7 Exceptions / Reports / Audit / Admin
- **Exceptions**: table of active exceptions with approver, expiry, reason; highlight expired.
- **Reports**: remediation by month, exceptions by team, open by severity, cycle-to-cycle trend; export CSV.
- **Audit Log**: chronological trail of uploads/status changes/approvals.
- **Admin**: users & roles (read + basic edit), scan-cycle reference.

## 5. UI Style

Enterprise theme, dark-neutral left rail + light content, Bootstrap 5 utilities.

| Token | Value | Use |
|-------|-------|-----|
| Background | `#f5f6f8` content / `#1f2733` rail | Shell |
| Critical | `#dc2626` | Critical badge |
| High | `#ea580c` | High badge |
| Medium | `#ca8a04` | Medium badge |
| Low | `#2563eb` | Low badge |
| Fixed | `#16a34a` | Fixed / success |
| Exception | `#eab308` | Exception / pending |
| Nav accent | `#2563eb` | Links, active item |

- Sticky table headers, zebra rows, expand/collapse for long plugin output.
- The **Vulnerability table is the primary working area** (bulk status filter/sort/search).
- Severity badges, status pills, KPI cards with subtle shadow.

## 6. Implementation Notes

- Front-end: ASP.NET Core MVC + Razor, Bootstrap 5 (already in `wwwroot/lib`), custom `site.css` theme.
  Charts are lightweight CSS/SVG (no external CDN) so the app runs offline.
- Data layer: `VRTrackingAppContext` registered in `Program.cs`. Default dev store is EF Core **InMemory**
  with seed data so the app runs with zero external dependencies; switch to SQL Server via
  `appsettings.json` connection string (documented in `Program.cs`).
- CSV ingestion: `NessusCsvParser` maps dynamic Nessus columns, validates required fields, and writes
  `ScanUpload` → `AssetHost` → `VulnerabilityFinding`/`VulnerabilityInstance`. Missing fields render as
  "Unavailable" rather than failing the import.

## 7. Build Order (executed incrementally)

1. Design doc (this file).
2. Extend `VRTrackingApp.Data` models + `VRTrackingAppContext`.
3. CSV parser + DI + seed data + InMemory/SQL switch.
4. Enterprise `_Layout` + theme.
5. Dashboard.
6. Upload (validation + parse status).
7. Scans / Hosts / Vulnerabilities (+ remediation panel).
8. Exceptions / Reports / Audit / Admin.
9. Build & verify.
