# Database Backup and Recovery Runbook
## RemediateVR

This runbook documents the procedures for backing up and recovering the RemediateVR SQL Server databases.

---

## Database Overview

| Database                | Purpose                                                  |
|-------------------------|----------------------------------------------------------|
| `RemediateVR`         | Primary application database (scans, assets, findings)   |
| `RemediateVR_Audit`   | Audit trail for regulatory and operational compliance    |

Both databases must be backed up to ensure a consistent recovery point.

---

## Automated Backups

A daily backup task is installed by `deployment\Install-DailyBackupTask.ps1`.

- **Schedule:** Daily at 02:00 AM
- **Location:** `C:\RemediateVR\Backups`
- **Format:** Compressed `.bak.gz`
- **Retention:** Move files older than 14 days to archive or delete per policy.

### Manual Backup

Run the backup script manually before major operations (e.g., schema migration):

```powershell
.\deployment\Backup-Databases.ps1 -ServerInstance "(local)\SQLEXPRESS" -Compress
```

### Verify Backup

```powershell
$latest = Get-ChildItem "C:\RemediateVR\Backups" -Filter "RemediateVR_*.gz" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host "Latest backup: $($latest.FullName) ($($latest.Length / 1MB) MB)"
```

---

## Recovery Procedures

### Scenario 1: Partial Corruption / Accidental Data Deletion

1. Identify the last known good backup.
2. Stop the IIS site (`Stop-WebSite -Name RemediateVR`).
3. Run restore:

```powershell
.\deployment\Restore-Databases.ps1 -ServerInstance "(local)\SQLEXPRESS" -DatabaseName "RemediateVR"
```

4. Start the IIS site (`Start-WebSite -Name RemediateVR`).
5. Verify `/api/health` returns `Healthy`.

### Scenario 2: Full Server Loss / Disaster Recovery

1. Provision a new Windows Server with IIS and SQL Server.
2. Install all prerequisites (see `production_environment_checklist.md`).
3. Restore `RemediateVR` first, then `RemediateVR_Audit`.
4. Publish the application (`deployment\Deploy-VRTrackingApp.ps1 -SkipPublish`).
5. Apply any post-restore data corrections (e.g., recalculation of aggregation tables).

---

## Restoration Testing

Test the restore procedure quarterly on a non-production server to confirm recovery times and backup integrity.

1. Restore from the latest backup to a sandbox.
2. Run the smoke tests in `VRTrackingApp.Tests`.
3. Confirm the UI loads and a file import succeeds.

---

## Contacts

| Role              | Contact |
|-------------------|---------|
| DBA / Infra Lead  | ___    |
| Engineering Lead  | ___    |
| On-Call (after-hours) | ___ |

*Runbook owner: Engineering*  
*Last review: 2026-07-23*
