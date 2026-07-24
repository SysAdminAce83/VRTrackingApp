# Quick Reference Guide
## RemediateVR

A one-page reference for common daily tasks.

---

## At-A-Glance

| Task                    | Path              | Shortcut / Tip                                   |
|-------------------------|-------------------|--------------------------------------------------|
| View dashboard          | Dashboard         | Home page                                         |
| Import scan             | Scans > Import    | CSV or PDF only                                   |
| Open finding detail     | Click a row       | Shows references, CVSS, status                    |
| Change remediation      | Status dropdown   | Add a comment                                     |
| Filter by severity      | Severity pills    | Red = Critical, Orange = High, etc.               |
| Export to CSV           | Export button     | Available on all list views                       |
| Search                  | Search bar        | Text search across host names, plugins, CVE IDs   |
| View audit trail        | Audit log         | Admin/Reviewer role required                      |
| Check notifications     | Bell icon (top)    | Click to view unread items                        |

---

## Remediation Statuses

| Status        | Meaning                                       |
|---------------|-----------------------------------------------|
| Open          | New; no action started yet                    |
| InProgress    | Being worked                                 |
| Fixed         | Patched and verified                          |
| Exception     | Risk accepted; reviewer approved              |
| Deferred      | Scheduled for future window                   |

---

## App Roles

| Role                 | What They Can Do                                           |
|----------------------|------------------------------------------------------------|
| Admin                | Full access + user enrollment + configuration              |
| Reviewer             | Read + status changes + approve exceptions                 |
| RemediationOwner     | Update status on findings assigned to them                 |

---

## Escalation

| Issue                          | Who to Contact            |
|--------------------------------|---------------------------|
| Cannot log in / Access Denied  | Administrator              |
| Application error 500          | Administrator / Engineering|
| Data discrepancy               | Engineering                |

---

*Keep this guide alongside your workstation.*
