# Support and Hypercare Runbook
## RemediateVR — Phase 6 Go-Live

This runbook defines the support posture during the hypercare period and ongoing operations after go-live.

---

## Hypercare Period

**Duration:** 2 weeks post go-live cutover

**Goals:**
- Resolve production issues rapidly.
- Monitor system health and usage.
- Capture feedback for post-launch enhancements.

**Support Hours:** Business hours (08:00 – 18:00 local), with on-call escalation outside these hours.

**Expected Response Times:**
- **P1 / Outage:** 30 minutes (pages on-call engineer)
- **P2 / Degraded:** 2 hours
- **P3 / Incidental:** Next business day

---

## On-Call Rotation

1. **Primary on-call:** Engineering lead (assigned at launch)
2. **Secondary on-call:** Senior developer
3. **Escalation to:** Product owner / Director of Engineering

---

## Monitoring Checklist (Daily)

| Check                                    | Owner       | Tool / Location              |
|------------------------------------------|-------------|------------------------------|
| `/api/health` returns `Healthy`           | On-call     | Browser / curl               |
| IIS app pool running                     | On-call     | IIS Manager / PowerShell     |
| SQL Server connectivity                  | On-call     | SSMS / `Invoke-Sqlcmd`       |
| Backup ran successfully at 02:00         | On-call     | Event Viewer / backup folder |
| Disk space on DB and publish drives      | On-call     | Windows Performance Monitor   |
| Event Viewer errors                      | On-call     | Event Viewer                 |

---

## Escalation Path

```
User reports issue
       |
       v
Support desk triages (log ticket, screenshot, browser info)
       |
       v
On-call engineer investigates (check logs, reproduce, hotfix if possible)
       |
       v
If cannot resolve within P1/P2 SLA -> escalate to secondary + engineering lead
```

---

## Common Incidents and Actions

| Symptom                           | Action                                                    |
|-----------------------------------|------------------------------------------------------------|
| 500 Internal Server Error          | Check `Event Viewer > Windows Logs > Application`. Examine `web.config` `stdoutLogEnabled` setting. |
| 403 Access Denied                  | Verify user is active in `UserAccounts` and has a role.    |
| Scan import slow / timeout         | Check CPU/memory on app server. Large PDFs may need more timeout. |
| Database connection loss           | Restart SQL Server service, verify connection string.       |
| Emails not delivered               | Check `appsettings.Production.json` SMTP block and event log. |

---

## Issue Intake

1. Open a ticket with:
   - User name and role
   - Timestamp (UTC if possible)
   - Error message and screenshot
   - Steps to reproduce
2. Assign priority per SLA matrix.
3. On-call engineer acknowledges and investigates.
4. Post-resolution: document in the ticket and update user.

---

## Post-Launch Enhancements

Feedback gathered during hypercare feeds into the backlog. Create GitHub issues (or ADO work items) tagged `post-launch` for each improvement request.

---

*Runbook owner: Engineering*  
*Effective: Go-live date*  
*Review: After hypercare period ends*
