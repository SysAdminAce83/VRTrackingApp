# End-User Training Materials
## RemediateVR

This document can be used as a slide deck or handout for user training sessions.

---

## Slide 1: Introduction

**RemediateVR** centralizes vulnerability findings from Nessus scans into a single platform for tracking remediation across assets.

**Key concepts:**
- **Scan:** A single Nessus export (CSV or PDF)
- **Asset:** A host discovered or referenced in a scan
- **Vulnerability / Finding:** A specific issue tied to a host
- **Remediation:** Work to fix an open finding (Patch, Exception, Deferred)

---

## Slide 2: Logging In

- Open the RemediateVR URL in a supported browser (Edge, Chrome).
- You are automatically signed in with your Windows (domain) credentials.
- If you see **Access Denied**, contact your administrator to enroll your domain account.

---

## Slide 3: Dashboard

- Shows aggregate metrics: total hosts, open vulnerabilities by severity, recent scans.
- Charts update as new scans are imported.

---

## Slide 4: Importing a Scan

1. Navigate to **Scans**.
2. Click **Import**.
3. Select a `.csv` or `.pdf` Nessus export.
4. Wait for processing to complete.
5. New assets and findings appear in the respective views.

---

## Slide 5: Reviewing Findings

- Navigate to **Vulnerabilities** or open a scan **Details** view.
- Use filters for severity, status, and host.
- Click a finding to view full details (Plugin, CVE, CVSS, references).

---

## Slide 6: Remediation Workflow

| Status        | Meaning                                        |
|---------------|------------------------------------------------|
| Open          | New finding; work not yet started              |
| InProgress    | Remediation in progress                        |
| Fixed         | Verified as patched                            |
| Exception     | Acceptable risk; approved by reviewer          |
| Deferred      | Planned for a future maintenance window        |

- To change status, open the finding and select an option from the **Status** dropdown.
- Add a comment to document the decision.

---

## Slide 7: Auditing and Reports

- All status changes, comments, and imports are tracked in the **Audit Trail**.
- Admins and reviewers can filter audit log by user, date, and entity type.
- Export CSV from any list view for offline reporting.

---

## Slide 8: Notifications

- In-app notification bell alerts you when:
  - A finding is escalated to your queue
  - An exception is awaiting your approval
- Optional email notifications can be enabled by your administrator.

---

## Slide 9: Accessibility and Support

- Keyboard navigation is supported for list views and dropdowns.
- Contact your administrator or the help desk for:
  - Access or enrollment issues
  - Bug reports
  - Training follow-up sessions

---

## Slide 10: Summary

| Action                       | Where                           |
|------------------------------|---------------------------------|
| Import a scan                | Scans > Import                  |
| Review a finding             | Vulnerabilities > Details       |
| Update remediation status    | Finding detail pane             |
| View audit history           | Audit log                       |
| Export data                  | Any list > Export CSV           |

---

*Training version: 1.0 — 2026-07-23*
