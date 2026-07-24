# Production Environment Checklist
## RemediateVR

This checklist is for the engineering team preparing the production environment before go-live.

---

### Checklist

#### Infrastructure
- [ ] IIS 10+ installed on Windows Server 2019/2022
- [ ] Application Pool `.NET CLR Version` set to **No Managed Code**
- [ ] Application Pool identity has read access to the published folder
- [ ] Windows Authentication enabled, Anonymous disabled (`web.config`)
- [ ] HTTPS binding configured (TLS 1.2+ minimum)
- [ ] HSTS header enabled (`app.UseHsts()`)
- [ ] Firewall rules allow 443 (and 80 if redirecting)

#### Database
- [ ] SQL Server 2019+ (or Azure SQL) provisioned
- [ ] Connection string in `appsettings.Production.json` with `UseInMemory=false`
- [ ] EF Core migrations applied (`dotnet ef database update`)
- [ ] Database user has `db_owner` on RemediateVR and RemediateVR_Audit
- [ ] Daily automated backup installed via `deployment\Install-DailyBackupTask.ps1`
- [ ] Backup retention policy set (recommend 14 days)
- [ ] Backup directory on a separate volume from data files

#### Security
- [ ] Server joined to Active Directory domain
- [ ] Service account created (least-privilege) for App Pool
- [ ] App Pool identity has no write access outside the published folder
- [ ] Uploads directory restricted by IIS Request Filtering / NTFS ACLs
- [ ] `ASPNETCORE_ENVIRONMENT` set to **Production**
- [ ] Secrets stored in environment variables / Azure Key Vault (not in files)
- [ ] CSRF and XSS protections validated
- [ ] Security headers applied (CSP, X-Frame-Options, Referrer-Policy)

#### Application
- [ ] Published folder verified (`dotnet publish --configuration Release`)
- [ ] `appsettings.Production.json` reviewed and deployed
- [ ] SQL connection string verified
- [ ] Email SMTP settings configured if `EmailChannel` is used
- [ ] RemediationMode set appropriately (`Simulated` or `Live`)
- [ ] Health endpoint reachable: `/api/health`
- [ ] Windows Event Log configured
- [ ] Custom error pages active (`/Home/Error`, `/Account/AccessDenied`)

#### Monitoring
- [ ] IIS Failed Request Tracing enabled
- [ ] Windows Performance Counters monitored (CPU, memory, app pool queues)
- [ ] SQL Server Agent alerts for job failures configured
- [ ] Health check endpoint polled by monitoring tool
- [ ] Disk space alerts configured for database and log drives

#### DNS and Load Balancer
- [ ] DNS record pointing to production server
- [ ] If load-balanced: sticky sessions or ARR configured

---

## Go-Live Day Steps

1. Run `deployment\Deploy-VRTrackingApp.ps1` to publish and configure IIS.
2. Run `deployment\Install-DailyBackupTask.ps1` to enable backups.
3. Verify `/api/health` returns `Healthy`.
4. Run a single test import through the web UI against production data.
5. Monitor Event Viewer and IIS logs for 4 hours post-cutover.

---

*Maintained by the Engineering Team*
