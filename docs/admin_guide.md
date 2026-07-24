# Administrator Guide
## RemediateVR

This guide is intended for system administrators responsible for operating RemediateVR after deployment.

---

## System Overview

RemediateVR is an on-premises vulnerability tracking system for ingesting Nessus scan data, cataloging findings, tracking remediation, and maintaining audit trails.

### Architecture

- **Frontend:** ASP.NET Core MVC (`VRTrackingApp.Web`), Windows Integrated Auth
- **API:** Controllers under `VRTrackingApp.Web/Controllers`
- **Data:** Entity Framework Core with SQL Server
- **Hosting:** IIS on Windows Server

---

## User Management

### Enrolling a New User

1. Add a row to `UserAccounts` in the database.
2. Set `IsActive = 1`.
3. Assign a role via `UserRoles`:
   - `Admin` (full access)
   - `Reviewer` (read + status changes)
   - `RemediationOwner` (remediation work)

### Removing a User

Set `IsActive = 0`. The next request will return 403 for the affected identity.

---

## Configuration

Key settings live in `appsettings.Production.json` and environment variables.

```json
{
  "ConnectionStrings": {
      "SqlServer": "Server=...;Database=RemediateVR;...",
    "UseInMemory": "false"
  },
  "Email": {
    "Smtp": {
      "Host": "smtp.corp.local",
      "Port": 25,
      "From": "vrtracking@corp.local"
    }
  },
  "RemediationOptions": {
    "SectionName": "Remediation",
    "Mode": "Simulated"
  }
}
```

| Setting                      | Description                                                |
|------------------------------|------------------------------------------------------------|
| `ConnectionStrings:UseInMemory` | Set to `false` for production SQL Server usage          |
| `ConnectionStrings:SqlServer`    | SQL Server connection string                              |
| `Email:Smtp:Host`                | SMTP relay for notifications                              |
| `Remediation:Mode`               | `Simulated` (dev/demo) or `Live` (WinRM/SSH automation)   |

---

## Publishing an Update

```powershell
.\deployment\Deploy-VRTrackingApp.ps1 -SiteName "RemediateVR" -SkipPublish
```

> Note: If you have already published (`dotnet publish`), pass `-SkipPublish` to skip republishing.

---

## Maintenance Windows

### Applying EF Core Migrations

```powershell
cd VRTrackingApp
dotnet ef database update --project src/VRTrackingApp.Data --startup-project VRTrackingApp.Web
```

### Restarting the Application Pool

```powershell
Restart-WebAppPool -Name "RemediateVRPool"
```

---

## Security

- The app uses Windows Integrated Authentication; no passwords are stored.
- Role changes in the database take effect after the next request.
- Store secrets in environment variables, not in `appsettings` files.
- Review `AuditLogs` for suspicious activity.

---

## Troubleshooting

| Symptom                           | Investigation                             | Resolution                            |
|-----------------------------------|-------------------------------------------|---------------------------------------|
| 500 errors on startup             | Event Viewer / `stdoutLogEnabled` in `web.config` | Fix startup exception                  |
| AccessDenied for valid domain user| Check `UserAccounts.IsActive` and `UserRoles` | Enroll user in DB                      |
| Backup script fails               | Check SQL Server permissions on the service account | Grant backup rights                   |
| Emails not sending                | Verify SMTP settings and firewall rules   | Correct `appsettings.Production.json` |

---

*Maintained by Engineering — updated 2026-07-23*
