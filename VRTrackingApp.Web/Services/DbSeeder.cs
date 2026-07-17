using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services.Exceptions;

namespace VRTrackingApp.Web.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(VRTrackingAppContext db)
    {
        if (await db.Roles.AnyAsync()) return;

        // Reset demo exceptions so the new owner/state fields are populated in the demo.
        if (await db.ExceptionRecords.AnyAsync())
        {
            db.ExceptionRecords.RemoveRange(db.ExceptionRecords);
            await db.SaveChangesAsync();
        }

        var adminRole = new Role { Name = "Admin", Description = "Full access" };
        var analystRole = new Role { Name = "Analyst", Description = "Review & triage findings" };
        var ownerRole = new Role { Name = "Remediation Owner", Description = "Owns remediation" };
        var auditorRole = new Role { Name = "Auditor", Description = "Read-only audit access" };
        var championRole = new Role { Name = "SecurityChampion", Description = "Approves exception changes" };
        var infraRole = new Role { Name = AppRoles.InfrastructureManager, Description = "Approves server/OS exceptions" };
        var netRole = new Role { Name = AppRoles.NetworkManager, Description = "Approves network/firewall exceptions" };
        var riskRole = new Role { Name = AppRoles.RiskCommittee, Description = "Approves exceptions at risk committee" };
        var cisoRole = new Role { Name = AppRoles.Ciso, Description = "Final exception approval authority" };
        db.Roles.AddRange(adminRole, analystRole, ownerRole, auditorRole, championRole, infraRole, netRole, riskRole, cisoRole);
        await db.SaveChangesAsync();

        var alice = new UserAccount { UserName = "asmith", Email = "asmith@corp.local", DisplayName = "Alice Smith", Role = adminRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var bob = new UserAccount { UserName = "bjones", Email = "bjones@corp.local", DisplayName = "Bob Jones", Role = ownerRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var carol = new UserAccount { UserName = "cwhite", Email = "cwhite@corp.local", DisplayName = "Carol White", Role = analystRole, Location = "Branch Office A", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        // Dave is a SecurityChampion (approves exception changes), scoped to the Corporate Datacenter location.
        var dave = new UserAccount { UserName = "daudit", Email = "daudit@corp.local", DisplayName = "Dave Auditor", Role = championRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var ian = new UserAccount { UserName = "imanager", Email = "imanager@corp.local", DisplayName = "Ian Manager", Role = infraRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var nina = new UserAccount { UserName = "nmanager", Email = "nmanager@corp.local", DisplayName = "Nina Manager", Role = netRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var ryan = new UserAccount { UserName = "rchair", Email = "rchair@corp.local", DisplayName = "Ryan Chair", Role = riskRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        var ciso = new UserAccount { UserName = "cciso", Email = "cciso@corp.local", DisplayName = "Cindy CISO", Role = cisoRole, Location = "Corporate Datacenter", MfaEnabled = true, MfaSecret = TotpService.GenerateSecret() };
        db.UserAccounts.AddRange(alice, bob, carol, dave, ian, nina, ryan, ciso);
        await db.SaveChangesAsync();

        db.AuditLogs.AddRange(
            new AuditLog { PerformedByUserId = alice.Id, PerformedByDisplayName = alice.DisplayName, Category = "User", Action = "Created", Target = "user 'bjones'", Detail = "Created Remediation Owner account.", PerformedAt = DateTime.UtcNow.AddDays(-12) },
            new AuditLog { PerformedByUserId = dave.Id, PerformedByDisplayName = dave.DisplayName, Category = "Exception", Action = "Approved", Target = "exception #3 (WEB-01)", Detail = "Approved SMBv1 mitigation exception for 90 days.", PerformedAt = DateTime.UtcNow.AddDays(-9) },
            new AuditLog { PerformedByUserId = carol.Id, PerformedByDisplayName = carol.DisplayName, Category = "User", Action = "Edited", Target = "user 'cwhite'", Detail = "Updated location to Branch Office A.", PerformedAt = DateTime.UtcNow.AddDays(-6) },
            new AuditLog { PerformedByUserId = dave.Id, PerformedByDisplayName = dave.DisplayName, Category = "Role", Action = "Edited", Target = "role 'Analyst'", Detail = "Granted access to Reports.", PerformedAt = DateTime.UtcNow.AddDays(-4) },
            new AuditLog { PerformedByUserId = alice.Id, PerformedByDisplayName = alice.DisplayName, Category = "Exception", Action = "Rejected", Target = "exception #5 (DC-01)", Detail = "Microcode update still pending.", PerformedAt = DateTime.UtcNow.AddDays(-2) },
            new AuditLog { PerformedByUserId = bob.Id, PerformedByDisplayName = bob.DisplayName, Category = "User", Action = "Deleted", Target = "user 'jpark'", Detail = "Offboarded leaver.", PerformedAt = DateTime.UtcNow.AddHours(-20) }
        );
        await db.SaveChangesAsync();

        var cycles = new[]
        {
            new { Label = "January Monthly", Date = new DateTime(2026,1,15), Type = "Monthly" },
            new { Label = "Feb Patch Tuesday", Date = new DateTime(2026,2,10), Type = "Patch Tuesday" },
            new { Label = "Q1 Risk-Based", Date = new DateTime(2026,3,5), Type = "Risk-based" },
        };

        var hostSpecs = new[]
        {
            ("WEB-01", "10.10.1.21", "Windows Server 2019"),
            ("WEB-02", "10.10.1.22", "Windows Server 2019"),
            ("DB-01", "10.10.2.10", "Red Hat Enterprise Linux 8"),
            ("APP-01", "10.10.3.15", "Ubuntu 22.04 LTS"),
            ("DC-01", "10.10.0.5", "Windows Server 2022"),
            ("FTP-01", "10.10.4.40", "Windows Server 2016"),
        };

        var findingSpecs = new (int Plugin, string Name, string? Cve, string Sev, double v3, double v2, string risk, string stig, string sol)[]
        {
            (11936, "SSL/TLS Weak Cipher Suites", "CVE-2015-2808", "Medium", 5.9, 4.3, "Medium", "CAT III", "Disable weak ciphers."),
            (10267, "RRD Reboot Required", null, "Low", 0.0, 0.0, "Low", "CAT IV", "Reboot after patching."),
            (10863, "SSL/TLS Certificate Signed Using Weak Hashing Algorithm", "CVE-2004-2761", "Medium", 5.9, 4.0, "Medium", "CAT III", "Reissue cert with SHA-256."),
            (19506, "Nessus Scan Information", null, "Info", 0.0, 0.0, "None", "CAT IV", "Informational only."),
            (11264, "Microsoft Windows SMBv1 Detection", "CVE-2017-0144", "High", 8.1, 7.5, "High", "CAT II", "Disable SMBv1."),
            (13848, "Microsoft Windows Remote Desktop Gateway RCE", "CVE-2020-0609", "Critical", 9.8, 9.3, "Critical", "CAT I", "Apply CVE-2020-0609 patch."),
            (20982, "Microsoft Windows Netlogon RCE", "CVE-2020-1472", "Critical", 10.0, 9.3, "Critical", "CAT I", "Install August 2020 rollup."),
            (21646, "Microsoft Windows DNS RCE", "CVE-2020-1350", "Critical", 9.8, 8.6, "Critical", "CAT I", "Apply SIGRed patch."),
            (151013, "Oracle Linux glibc RCE", "CVE-2015-7547", "High", 7.5, 6.8, "High", "CAT II", "Upgrade glibc."),
            (17861, "OpenSSH Information Disclosure", "CVE-2018-15473", "Low", 3.1, 3.5, "Low", "CAT IV", "Upgrade OpenSSH."),
            (61521, "Adobe Reader Out-of-Bounds Write", "CVE-2021-44724", "High", 7.8, 6.8, "High", "CAT II", "Update Adobe Reader."),
        };

        foreach (var c in cycles)
        {
            var scan = new ScanUpload
            {
                FileName = $"nessus_{c.Type}_{c.Date:yyyyMMdd}.csv",
                FileHash = Guid.NewGuid().ToString("N"),
                FileSize = 42_000,
                Status = "Completed",
                ScanCycleLabel = c.Label,
                ScanDate = c.Date,
                SourceType = c.Type,
                Notes = $"Automated {c.Type} scan import.",
                UploadedBy = alice,
                UploadedAt = c.Date.AddHours(2)
            };
            db.ScanUploads.Add(scan);
            db.UploadAuditTrails.Add(new UploadAuditTrail
            {
                ScanUploadId = scan.Id,
                Action = "Uploaded",
                PerformedByUserId = alice.Id,
                PerformedAt = scan.UploadedAt
            });
            db.UploadAuditTrails.Add(new UploadAuditTrail
            {
                ScanUploadId = scan.Id,
                Action = "Parsed & validated",
                PerformedByUserId = carol.Id,
                PerformedAt = scan.UploadedAt.AddMinutes(5)
            });

            // Deterministic host + finding assignment.
            var rnd = new Random(c.Date.GetHashCode());
            foreach (var h in hostSpecs)
            {
                var (category, subCategory) = ClassifyAsset(h.Item1, h.Item3);
                var asset = new Asset
                {
                    HostName = h.Item1,
                    IpAddress = h.Item2,
                    OperatingSystem = h.Item3,
                    Location = "Corporate Datacenter",
                    Datacenter = "Corporate Datacenter",
                    AssetStatus = "Active",
                    Environment = "Production",
                    Category = category,
                    SubCategory = subCategory,
                    BiaCriticality = h.Item1.StartsWith("DC") ? "Critical" : "High",
                    AssetOwner = carol.DisplayName,
                    BusinessOwner = carol.DisplayName,
                    FirstSeen = c.Date,
                    LastSeen = c.Date,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Assets.Add(asset);

                var host = new AssetHost
                {
                    ScanUpload = scan,
                    HostName = h.Item1,
                    IpAddress = h.Item2,
                    OperatingSystem = h.Item3,
                    Asset = asset,
                    CreatedAt = DateTime.UtcNow
                };
                db.AssetHosts.Add(host);

                foreach (var f in findingSpecs)
                {
                    if (rnd.Next(0, 10) > 6) continue; // not every host has every finding

                    var finding = await FindOrAddFindingAsync(db, findingSpecs, f);
                    var statuses = new[] { "Open", "Open", "Fixed", "Exception" };
                    var status = statuses[rnd.Next(statuses.Length)];

                    var instance = new VulnerabilityInstance
                    {
                        AssetHost = host,
                        VulnerabilityFinding = finding,
                        Port = f.Sev == "Critical" || f.Sev == "High" ? 445 : 443,
                        Protocol = "tcp",
                        ServiceName = f.Sev == "Critical" || f.Sev == "High" ? "microsoft-ds" : "https",
                        PluginOutput = $"Remote detection of {f.Name} on {h.Item1}.",
                        Status = status,
                        Owner = status == "Open" ? bob : (status == "Exception" ? carol : alice),
                        DueDate = c.Date.AddDays(30),
                        FirstFound = c.Date,
                        LastFound = c.Date
                    };
                    db.VulnerabilityInstances.Add(instance);

                    db.RemediationActions.Add(new RemediationAction
                    {
                        VulnerabilityInstance = instance,
                        Action = status,
                        Status = status,
                        AssignedTo = instance.Owner,
                        DueDate = instance.DueDate,
                        Comments = status == "Exception" ? "Compensating controls in place." : null,
                        PerformedBy = alice,
                        CreatedAt = c.Date.AddDays(1)
                    });

                    if (status == "Exception")
                    {
                        db.ExceptionRecords.Add(new ExceptionRecord
                        {
                            VulnerabilityInstance = instance,
                            Reason = "Patch deferred; compensating control approved.",
                            ApprovedBy = carol,
                            Owner = bob,
                            State = ExceptionStates.Active,
                            ExpiresAt = c.Date.AddDays(90),
                            CreatedAt = c.Date.AddDays(1)
                        });
                    }
                }
            }
        }

        await db.SaveChangesAsync();

        // =====================================================================
        // V2 exception demo records (enterprise workflow + approval chain).
        // These illustrate: an active exception, a pending request, and a
        // rejected request. They reuse vulnerability instances seeded above.
        // =====================================================================
        await SeedV2ExceptionsAsync(db, new ExceptionWorkflowService(), carol, bob, ian, nina, ryan, ciso, dave);
        await db.SaveChangesAsync();
    }

    private static async Task SeedV2ExceptionsAsync(
        VRTrackingAppContext db,
        ExceptionWorkflowService wf,
        UserAccount carol, UserAccount bob, UserAccount ian, UserAccount nina,
        UserAccount ryan, UserAccount ciso, UserAccount dave)
    {
        VulnerabilityInstance? FindInst(string pluginName, string host) =>
            db.VulnerabilityInstances
                .Include(i => i.VulnerabilityFinding)
                .Include(i => i.AssetHost).ThenInclude(h => h.Asset)
                .FirstOrDefault(i => i.VulnerabilityFinding!.PluginName == pluginName && i.AssetHost!.HostName == host);

        // --- 1. SMBv1 (active exception, Infrastructure Manager chain) ---
        var smb = FindInst("Microsoft Windows SMBv1 Detection", "WEB-01");
        if (smb != null)
        {
            var ex = BuildBaseRequest(smb, carol, bob, NonFixableReason.LegacyApplicationDependency,
                "Manufacturing application requires SMBv1; vendor ETA Q2 2027.",
                "Application team cannot schedule downtime; production line runs 24x7.",
                Likelihood.High, ImpactLevel.Medium);
            ex.Status = ExceptionStatus.ActiveException;
            ex.Stage1Role = AppRoles.InfrastructureManager;
            ex.ApprovedAt = DateTime.UtcNow.AddDays(-20);
            ex.StartDate = DateTime.UtcNow.AddDays(-20);
            ex.ExpiryDate = DateTime.UtcNow.AddDays(70);
            ex.ReviewFrequencyDays = 90;
            ex.NextReviewDate = DateTime.UtcNow.AddDays(70);
            ex.VulnerabilityInstance!.Status = "Exception";
            ex.AffectsConfidentiality = true;
            ex.AffectsIntegrity = true;
            ex.Exploitability = Exploitability.PublicExploit;
            ex.InternetExposure = InternetExposure.InternalOnly;
            foreach (var s in new[]
            {
                new ExceptionApprovalStep { StepOrder = 1, Stage = ApprovalStage.Technical, RequiredRole = AppRoles.InfrastructureManager, Decision = ApprovalDecision.Approved, DecisionByUserId = ian.Id, DecisionAt = DateTime.UtcNow.AddDays(-19), Comment = "Mitigations verified." },
                new ExceptionApprovalStep { StepOrder = 2, Stage = ApprovalStage.Manager, RequiredRole = AppRoles.RiskCommittee, Decision = ApprovalDecision.Approved, DecisionByUserId = ryan.Id, DecisionAt = DateTime.UtcNow.AddDays(-18), Comment = "Accepted." },
                new ExceptionApprovalStep { StepOrder = 3, Stage = ApprovalStage.Security, RequiredRole = AppRoles.Ciso, Decision = ApprovalDecision.Approved, DecisionByUserId = ciso.Id, DecisionAt = DateTime.UtcNow.AddDays(-17), Comment = "Final approval granted." },
            })
                ex.ApprovalSteps.Add(s);
            foreach (var m in new[]
            {
                new ExceptionMitigation { Description = "Disable SMBv1 on non-required hosts", Status = MitigationStatus.Implemented },
                new ExceptionMitigation { Description = "EDR monitoring enabled", Status = MitigationStatus.Implemented },
                new ExceptionMitigation { Description = "Restrict RDP", Status = MitigationStatus.Pending },
            })
                ex.Mitigations.Add(m);
            ex.SecurityControls.Add(new ExceptionSecurityControl { ControlName = "Firewall" });
            ex.SecurityControls.Add(new ExceptionSecurityControl { ControlName = "EDR" });
            ex.Evidence.Add(new ExceptionEvidence { EvidenceType = EvidenceType.FirewallScreenshot, OriginalFileName = "fw-rule.png", StoredFileName = "seed_firewall.png", SizeBytes = 12345, UploadedByUserId = carol.Id });
            ex.Evidence.Add(new ExceptionEvidence { EvidenceType = EvidenceType.EdrScreenshot, OriginalFileName = "edr-mon.png", StoredFileName = "seed_edr.png", SizeBytes = 9876, UploadedByUserId = carol.Id });
            ex.Comments.Add(new ExceptionComment { UserId = bob.Id, AuthorDisplayName = bob.DisplayName, Body = "Vendor confirmed no patch until Q2 2027.", CreatedAt = DateTime.UtcNow.AddDays(-15) });
            ex.Reviews.Add(new ExceptionReviewHistory { DueDate = ex.NextReviewDate.Value, Outcome = ReviewOutcome.Pending });
            ex.VendorResponses.Add(new VendorResponse { Vendor = "Microsoft", ResponseText = "No out-of-band fix; track for next cumulative update.", PatchEtaDate = new DateTime(2027, 4, 1) });
            db.ExceptionRecords.Add(ex);
        }

        // --- 2. SSL/TLS weak ciphers (pending technical approval, Network Manager chain) ---
        var tls = FindInst("SSL/TLS Weak Cipher Suites", "WEB-01");
        if (tls != null)
        {
            var ex = BuildBaseRequest(tls, carol, bob, NonFixableReason.OperationalConstraint,
                "Load balancer requires maintenance window to rotate ciphers.",
                "Customer-facing site; downtime requires CAB approval.",
                Likelihood.Medium, ImpactLevel.Medium);
            wf.StartApproval(ex, AppRoles.NetworkManager);
            ex.VulnerabilityInstance!.Status = "Open";
            ex.AffectsConfidentiality = true;
            ex.AffectsIntegrity = false;
            ex.Exploitability = Exploitability.NoExploit;
            ex.InternetExposure = InternetExposure.InternetFacing;
            ex.SecurityControls.Add(new ExceptionSecurityControl { ControlName = "WAF" });
            ex.Mitigations.Add(new ExceptionMitigation { Description = "Schedule cipher rotation via CAB", Status = MitigationStatus.Pending });
            db.ExceptionRecords.Add(ex);
        }

        // --- 3. Netlogon RCE (rejected) ---
        var netlogon = FindInst("Microsoft Windows Netlogon RCE", "DC-01");
        if (netlogon != null)
        {
            var ex = BuildBaseRequest(netlogon, carol, bob, NonFixableReason.VendorPatchNotAvailable,
                "Microcode update pending from hardware vendor.",
                "Domain controller; cannot be offline during business hours.",
                Likelihood.Critical, ImpactLevel.Critical);
            ex.Stage1Role = AppRoles.InfrastructureManager;
            ex.Status = ExceptionStatus.Rejected;
            ex.RejectionReason = "Microcode update still pending; reapply after patch available.";
            ex.VulnerabilityInstance!.Status = "Open";
            ex.AffectsConfidentiality = true;
            ex.AffectsIntegrity = true;
            ex.AffectsAvailability = true;
            ex.Exploitability = Exploitability.PublicExploit;
            ex.InternetExposure = InternetExposure.InternalOnly;
            foreach (var s in new[]
            {
                new ExceptionApprovalStep { StepOrder = 1, Stage = ApprovalStage.Technical, RequiredRole = AppRoles.InfrastructureManager, Decision = ApprovalDecision.Approved, DecisionByUserId = ian.Id, DecisionAt = DateTime.UtcNow.AddDays(-6), Comment = "Confirmed." },
                new ExceptionApprovalStep { StepOrder = 2, Stage = ApprovalStage.Manager, RequiredRole = AppRoles.RiskCommittee, Decision = ApprovalDecision.Rejected, DecisionByUserId = ryan.Id, DecisionAt = DateTime.UtcNow.AddDays(-5), Comment = "Microcode update still pending." },
                new ExceptionApprovalStep { StepOrder = 3, Stage = ApprovalStage.Security, RequiredRole = AppRoles.Ciso, Decision = ApprovalDecision.Pending },
            })
                ex.ApprovalSteps.Add(s);
            db.ExceptionRecords.Add(ex);
        }
    }

    private static ExceptionRecord BuildBaseRequest(
        VulnerabilityInstance inst, UserAccount requester, UserAccount owner,
        NonFixableReason reason, string tech, string business,
        Likelihood likelihood, ImpactLevel impact)
    {
        var risk = RiskMatrixService.Calculate(likelihood, impact);
        return new ExceptionRecord
        {
            VulnerabilityInstance = inst,
            Reason = tech,
            OwnerUserId = owner.Id,
            NonFixableReason = reason,
            TechnicalJustification = tech,
            DowntimeConstraint = business,
            BusinessImpact = business,
            Likelihood = likelihood,
            Impact = impact,
            OverallRisk = risk,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Status = ExceptionStatus.ExceptionRequested
        };
    }

    private static (string Category, string SubCategory) ClassifyAsset(string hostName, string os)
    {
        var h = hostName.ToUpperInvariant();
        var category = h switch
        {
            var x when x.StartsWith("DC") => "Server",
            var x when x.StartsWith("DB") => "Database",
            var x when x.StartsWith("WEB") => "Server",
            var x when x.StartsWith("APP") => "Application",
            var x when x.StartsWith("FTP") => "Server",
            _ => "Server"
        };
        var sub = h switch
        {
            var x when x.StartsWith("DC") => "Domain Controller",
            var x when x.StartsWith("DB") => "Database Server",
            var x when x.StartsWith("WEB") => "Web Server",
            var x when x.StartsWith("APP") => "Application Server",
            var x when x.StartsWith("FTP") => "File Server",
            _ => "Generic Server"
        };
        return (category, sub);
    }

    private static async Task<VulnerabilityFinding> FindOrAddFindingAsync(
        VRTrackingAppContext db,
        (int Plugin, string Name, string? Cve, string Sev, double v3, double v2, string risk, string stig, string sol)[] specs,
        (int Plugin, string Name, string? Cve, string Sev, double v3, double v2, string risk, string stig, string sol) f)
    {
        var existing = await db.VulnerabilityFindings.FirstOrDefaultAsync(x => x.PluginId == f.Plugin);
        if (existing != null) return existing;

        var finding = new VulnerabilityFinding
        {
            PluginId = f.Plugin,
            PluginName = f.Name,
            Cve = f.Cve,
            Severity = f.Sev,
            RiskFactor = f.risk,
            Synopsis = $"A {f.Sev.ToLower()} severity issue: {f.Name}.",
            Description = $"Nessus plugin {f.Plugin} reports {f.Name} on the target host. " +
                          $"Risk factor: {f.risk}.",
            Solution = f.sol,
            CvssV3BaseScore = f.v3,
            CvssV2BaseScore = f.v2,
            CvssV3TemporalScore = Math.Round(f.v3 * 0.9, 1),
            CvssV2TemporalScore = Math.Round(f.v2 * 0.9, 1),
            VprScore = f.Sev == "Critical" ? 9.5 : f.Sev == "High" ? 7.0 : 4.0,
            EpssScore = f.Sev == "Critical" ? 0.94 : f.Sev == "High" ? 0.6 : 0.1,
            StigSeverity = f.stig,
            References = f.Cve != null ? $"https://nvd.nist.gov/vuln/detail/{f.Cve}" : "https://www.tenable.com/plugins/nessus/" + f.Plugin,
            CreatedAt = DateTime.UtcNow
        };
        db.VulnerabilityFindings.Add(finding);
        return finding;
    }
}
