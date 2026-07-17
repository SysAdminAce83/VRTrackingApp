# Exception Module V2 — Design & Implementation Plan

> Companion to `gui_design.md`, `architecture.md`, `database_schema.md`.
> Scope: redesign the Exception module and the overall vulnerability exception
> lifecycle in `VRTrackingApp.Web` into an enterprise-grade, evidence-driven,
> multi-stage approval workflow suitable for ISO 27001 / PCI DSS / NIST audits.

## 1. Decisions (confirmed with product owner)

1. **Approval chain** — dynamic first stage by vulnerability/asset type, then fixed escalation:
   - **Stage 1: Infrastructure Manager** — for Server / OS / application-on-server vulnerabilities.
   - **Stage 1 (alt): Network Manager** — for Network / Firewall / Security-appliance / network-service vulnerabilities.
   - **Stage 2: Risk Committee.**
   - **Stage 3: CISO** (final approval).
2. **Approver identity = by role.** New roles added: `InfrastructureManager`, `NetworkManager`, `RiskCommittee`, `CISO`. Anyone holding the stage's role may act on that stage.
3. **Notifications = In-app + Email (SMTP).** In-app always on; email sent when SMTP is configured in `appsettings.json` (no-op otherwise).
4. **Creation flow = replace** the old "set Status = Exception" dropdown on Vulnerability Details with a formal multi-section **Request Exception** form that must pass approval before the instance is marked `Exception`.
5. **Persistence** — EF InMemory remains the dev default; a migration (`ExceptionModuleV2`) is added for the SQL Server target.

## 2. Stage-1 routing rule (Infra vs Network)

Determined by `ExceptionRoutingService` in this order:
1. **Asset category** (`Asset.Category` / `SubCategory`): `Network`, `Firewall`, `Router`, `Switch`, `Load Balancer` → **NetworkManager**.
2. **Vulnerability keywords** (in `PluginName` / `RiskFactor` / finding text): `firewall, network, cipher, tls, ssl, smb, rdp, dns, vpn, port, protocol, ids, ips, waf, certificate, netlogon` → **NetworkManager**.
3. Otherwise (Server, Database, Application, OS patch, etc.) → **InfrastructureManager**.

The resolved manager role is stored on the exception (`Stage1Role`) so the chain is auditable and stable even if classification rules change later.

## 3. Status lifecycle (state machine)

Persisted `ExceptionStatus` enum, owned entirely by `ExceptionWorkflowService`:

```
Draft ─► Submitted ─► PendingManagerApproval ─► PendingRiskApproval ─► PendingCisoApproval ─► Approved ─► Active
                                                                                                   │
Active ─► ReviewDue ─► (Renewed ─► Active)  |  Expired  |  Closed{Patched|Mitigated|FalsePositive|RiskRemoved}
```

Off any `Pending*` stage: **Rejected** (terminal) or **NeedMoreInfo** (returns to requester → resubmit). `CurrentApprovalStage` points at the active stage; `Status` is derived from chain progress. The linked `VulnerabilityInstance.Status` becomes `Exception` only once the exception reaches **Active**; while pending it stays `Open` (flagged "Exception Requested").

## 4. Data model

### 4.1 Extend `ExceptionRecord`
Lifecycle: `Status` (enum), `Stage1Role`, `CurrentApprovalStage`, `SubmittedAt`, `ApprovedAt`, `ClosedReason`, `ClosedAt`.
S2: `NonFixableReason` (enum), `OtherReasonText`.
S3: `TechnicalJustification`.
S4: `DowntimeConstraint`, `BusinessImpact`, `CostImpact`, `ProductionImpact`, `CustomerImpact`, `ComplianceImpact`.
S5: `Likelihood` (enum), `Impact` (enum), `OverallRisk` (enum, auto).
S6: `AffectsConfidentiality`, `AffectsIntegrity`, `AffectsAvailability` (bool).
S7: `Exploitability` (enum). S8: `InternetExposure` (enum).
S13: `StartDate`, `ExpiryDate`, `ReviewFrequencyDays`, `NextReviewDate`.
(existing `Reason`, `ExpiresAt`→ superseded by `ExpiryDate`, `OwnerUserId`, approver fields retained for back-compat.)

### 4.2 New tables
- `ExceptionMitigation` — { ExceptionRecordId, Description, Status(enum) }
- `ExceptionEvidence` — { ExceptionRecordId, EvidenceType(enum), OriginalFileName, StoredFileName, ContentHash, SizeBytes, UploadedByUserId, UploadedAt }
- `ExceptionSecurityControl` — { ExceptionRecordId, ControlName } (from static control catalog)
- `ExceptionApprovalStep` — { ExceptionRecordId, StepOrder, Stage(enum), RequiredRole, Decision(enum), DecisionByUserId, DecisionAt, Comment }
- `ExceptionReviewHistory` — { ExceptionRecordId, DueDate, ReviewedAt, ReviewedByUserId, Outcome(enum), Comment }
- `ExceptionComment` — { ExceptionRecordId, UserId, Body, CreatedAt }
- `Notification` — { UserId, Type(enum), ExceptionRecordId?, Title, Message, Channel, IsRead, CreatedAt, EmailedAt? }
- `VendorResponse` — { ExceptionRecordId, Vendor, ResponseText, PatchEtaDate?, ReceivedAt }

### 4.3 Extend `AuditLog`
Add `OldValue`, `NewValue`, `IpAddress` to satisfy the audit-trail spec (user / date / action / old / new / IP / comment).

### 4.4 Enums (stored as strings via `HasConversion<string>()`)
`ExceptionStatus`, `ApprovalStage`, `ApprovalDecision`, `NonFixableReason`, `Likelihood`, `Impact`, `RiskLevel`, `Exploitability`, `InternetExposure`, `MitigationStatus`, `ReviewOutcome`, `EvidenceType`, `NotificationType`.

## 5. Risk matrix (`RiskMatrixService`)

Likelihood (VeryLow…Critical) × Impact (Low…Critical) → `RiskLevel`. Auto-calculated on the form and stored in `OverallRisk`. Default matrix biases toward the higher of the two axes with escalation when both are high.

## 6. Roles & seed data

Add roles `InfrastructureManager`, `NetworkManager`, `RiskCommittee`, `CISO` + demo users for each. Seed 2–3 demo V2 exceptions in varied states (pending manager, active, review-due) with populated approval steps so the dashboard/detail/list are populated.

## 7. UI (P1 scope)

- **Request Exception form** (`Exceptions/Request`) — 14 collapsible sections; S1 auto-filled read-only; S5 auto-calc risk; S9 checkbox catalog; S10 repeatable mitigation rows; S11 multi-file evidence.
- **Exception list** (`Exceptions/Index`) — status/severity/risk/owner/expiry filters, status pills.
- **Exception Detail** (`Exceptions/Details`) — all sections + approval history + timeline + comments + audit + linked vuln.
- **Vulnerability Details** — replace the Exception status option with a **Request Exception** button → prefilled form.

## 8. Approval, review, notifications (P2–P5)

- P2: approver actions Approve / Reject / Need-Info (mandatory comment) advancing the chain via the workflow service.
- P3: Exception Dashboard KPIs.
- P4: hosted `ExceptionLifecycleService` — Active→ReviewDue→Expired, reminders 30/15/7 days, overdue-mitigation / missing-evidence flags.
- P5: `Notification` + `INotificationService` (in-app + SMTP email provider).

## 9. Cross-cutting cleanups

- Fix `[Authorize]` on `ExceptionsController` so owners/requesters can use the request flow (approver-only actions gated per-action).
- Real enums for status/decision (no more free strings).
- Unify expiry (`ExpiryDate`) and deprecate `RemediationAction.ExceptionExpiryDate` drift.
- Audit creation + expiry (currently unlogged).

## 10. Phased roadmap

P0 model+migration+services+seed → P1 request form + list + detail + creation entry point → P2 approval chain actions → P3 dashboard → P4 review/expiry jobs → P5 notifications → P6 evidence/mitigation/vendor mgmt → P7 auditor reports → P8 (nice-to-have) auto-close on rescan, ServiceNow/Jira links, trends.

## 11. Known data gaps (render "Unavailable" or add later)

- `VulnerabilityFinding` has no **Published Date**.
- `Asset` has no explicit **Technical Owner** (map to `AssetOwner` / `InternalPoc`).
