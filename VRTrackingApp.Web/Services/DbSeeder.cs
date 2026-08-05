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

        // ---- Compliance / GRC Seed Data ----
        await SeedComplianceDataAsync(db);

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

    private static async Task SeedComplianceDataAsync(VRTrackingAppContext db)
    {
        if (await db.Frameworks.AnyAsync()) return;

        var cisV8 = new Framework
        {
            Name = "CIS Controls v8",
            ShortName = "CIS Controls v8",
            Version = "8.0",
            Description = "The CIS Controls v8 framework provides a prioritized set of actions to protect organizations and data from known attack vectors.",
            CreatedAt = DateTime.UtcNow
        };
        var nistCsf = new Framework
        {
            Name = "NIST Cybersecurity Framework",
            ShortName = "NIST CSF",
            Version = "2.0",
            Description = "The NIST CSF provides a policy framework of computer security guidance for how private sector organizations can assess and improve their ability to prevent, detect, and respond to cyber attacks.",
            CreatedAt = DateTime.UtcNow
        };
        var nist800_53 = new Framework
        {
            Name = "NIST SP 800-53 Rev 5",
            ShortName = "NIST 800-53",
            Version = "Rev 5",
            Description = "Security and Privacy Controls for Information Systems and Organizations.",
            CreatedAt = DateTime.UtcNow
        };
        var iso27001 = new Framework
        {
            Name = "ISO/IEC 27001:2022",
            ShortName = "ISO 27001",
            Version = "2022",
            Description = "Information security management systems requirements.",
            CreatedAt = DateTime.UtcNow
        };
        var pciDss = new Framework
        {
            Name = "PCI DSS v4.0",
            ShortName = "PCI-DSS",
            Version = "4.0",
            Description = "Payment Card Industry Data Security Standard.",
            CreatedAt = DateTime.UtcNow
        };
        db.Frameworks.AddRange(cisV8, nistCsf, nist800_53, iso27001, pciDss);
        await db.SaveChangesAsync();

        var cisFamilies = new[]
        {
            new ControlFamily { FamilyId = "CIS-IG", Name = "Governance and Culture", Description = "Establish and manage cybersecurity risk management strategy.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "CIS-IM", Name = "Asset Management", Description = "Manage assets to establish cybersecurity risk management.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "CIS-PR", Name = "Protection", Description = "Implement safeguards to ensure delivery of critical services.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "CIS-DE", Name = "Detection", Description = "Implement activities to identify cybersecurity events.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "CIS-RS", Name = "Response", Description = "Implement activities to take action regarding detected incidents.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "CIS-REC", Name = "Recovery", Description = "Implement activities to maintain plans and restore capabilities.", FrameworkId = cisV8.Id, CreatedAt = DateTime.UtcNow },
        };
        db.ControlFamilies.AddRange(cisFamilies);

        var nistFamilies = new[]
        {
            new ControlFamily { FamilyId = "NIST-GOV", Name = "Govern", Description = "Establish cybersecurity risk management strategy.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "NIST-IDENT", Name = "Identify", Description = "Understand the organization's cybersecurity risk.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "NIST-PROTECT", Name = "Protect", Description = "Implement safeguards to ensure delivery of critical services.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "NIST-DETECT", Name = "Detect", Description = "Identify cybersecurity events in a timely manner.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "NIST-RESPOND", Name = "Respond", Description = "Take action regarding detected cybersecurity incidents.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "NIST-RECOVER", Name = "Recover", Description = "Restore capabilities impaired by cybersecurity incidents.", FrameworkId = nistCsf.Id, CreatedAt = DateTime.UtcNow },
        };
        db.ControlFamilies.AddRange(nistFamilies);

        var nist800Families = new[]
        {
            new ControlFamily { FamilyId = "N800-AC", Name = "Access Control", Description = "Controls to limit access to information systems.", FrameworkId = nist800_53.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "N800-AU", Name = "Audit and Accountability", Description = "Capture and examine audit records.", FrameworkId = nist800_53.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "N800-IR", Name = "Incident Response", Description = "Manage and respond to cybersecurity incidents.", FrameworkId = nist800_53.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "N800-SC", Name = "System and Communications Protection", Description = "Protect information at rest and in transit.", FrameworkId = nist800_53.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "N800-SI", Name = "System and Information Integrity", Description = "Detect, respond to, and correct security weaknesses.", FrameworkId = nist800_53.Id, CreatedAt = DateTime.UtcNow },
        };
        db.ControlFamilies.AddRange(nist800Families);

        var isoFamilies = new[]
        {
            new ControlFamily { FamilyId = "ISO-ORG", Name = "Organizational Controls", Description = "Controls at the organizational level.", FrameworkId = iso27001.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "ISO-PEOPLE", Name = "People Controls", Description = "Controls related to human factors.", FrameworkId = iso27001.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "ISO-PHYS", Name = "Physical Controls", Description = "Controls to protect physical assets.", FrameworkId = iso27001.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "ISO-TECH", Name = "Technological Controls", Description = "Controls using technology mechanisms.", FrameworkId = iso27001.Id, CreatedAt = DateTime.UtcNow },
        };
        db.ControlFamilies.AddRange(isoFamilies);

        var pciFamilies = new[]
        {
            new ControlFamily { FamilyId = "PCI-REQ1", Name = "Build and Maintain Secure Networks", Description = "Install and maintain network security controls.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "PCI-REQ2", Name = "Protect Cardholder Data", Description = "Protect stored and transmitted cardholder data.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "PCI-REQ3", Name = "Maintain Vulnerability Management", Description = "Protect systems against malware and vulnerabilities.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "PCI-REQ4", Name = "Implement Strong Access Controls", Description = "Restrict access to cardholder data.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "PCI-REQ5", Name = "Regularly Monitor and Test Networks", Description = "Monitor and test networks to ensure security.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
            new ControlFamily { FamilyId = "PCI-REQ6", Name = "Maintain Information Security Policies", Description = "Maintain policies that support information security.", FrameworkId = pciDss.Id, CreatedAt = DateTime.UtcNow },
        };
        db.ControlFamilies.AddRange(pciFamilies);
        await db.SaveChangesAsync();

        var cisControls = new[]
        {
            new ComplianceControl { ControlId = "CIS-1.1", Name = "Establish an Asset Inventory", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[1].Id, Description = "Maintain an accurate and complete inventory of assets.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-1.2", Name = "Establish an Asset Inventory Process", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[1].Id, Description = "Define and maintain a process for asset inventory.", Impact = ComplianceImpact.Medium, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-2.1", Name = "Establish a Software Inventory", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[1].Id, Description = "Maintain an accurate and complete inventory of software.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-3.1", Name = "Establish Secure Configurations", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[2].Id, Description = "Define and implement secure configurations for assets.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-4.1", Name = "Continuous Vulnerability Management", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[2].Id, Description = "Continuously identify, assess, and remediate vulnerabilities.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-5.1", Name = "Account Management", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[2].Id, Description = "Manage accounts to ensure authorized access.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-5.2", Name = "Authentication Management", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[2].Id, Description = "Implement strong authentication mechanisms.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-7.1", Name = "Continuous Data Protection", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[2].Id, Description = "Implement continuous data protection measures.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-8.1", Name = "Audit Log Management", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[3].Id, Description = "Collect and manage audit logs for detection.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-8.2", Name = "Event Logging", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[3].Id, Description = "Log security-relevant events across the organization.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-9.1", Name = "Incident Response Management", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[4].Id, Description = "Manage incident response processes.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "CIS-10.1", Name = "Incident Response Plan", Framework = "CIS Controls v8", ControlFamilyId = cisFamilies[5].Id, Description = "Maintain and test incident response plans.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
        };
        db.ComplianceControls.AddRange(cisControls);

        var nistCsfControls = new[]
        {
            new ComplianceControl { ControlId = "NIST-CSF-GOV-1", Name = "Identify Cybersecurity Risk", Framework = "NIST CSF", ControlFamilyId = nistFamilies[0].Id, Description = "Understand the organization's cybersecurity risk.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-CSF-IDENT-1", Name = "Asset Management", Framework = "NIST CSF", ControlFamilyId = nistFamilies[1].Id, Description = "Identify and manage assets across the organization.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-CSF-PROTECT-1", Name = "Identity Management and Access Control", Framework = "NIST CSF", ControlFamilyId = nistFamilies[2].Id, Description = "Implement identity management and access control.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-CSF-DETECT-1", Name = "Continuous Monitoring", Framework = "NIST CSF", ControlFamilyId = nistFamilies[3].Id, Description = "Implement continuous monitoring for security events.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-CSF-RESPOND-1", Name = "Incident Response", Framework = "NIST CSF", ControlFamilyId = nistFamilies[4].Id, Description = "Implement incident response capabilities.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-CSF-RECOVER-1", Name = "Recovery Planning", Framework = "NIST CSF", ControlFamilyId = nistFamilies[5].Id, Description = "Plan and implement recovery procedures.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
        };
        db.ComplianceControls.AddRange(nistCsfControls);

        var nist800Controls = new[]
        {
            new ComplianceControl { ControlId = "NIST-AC-2", Name = "Account Management", Framework = "NIST 800-53", ControlFamilyId = nist800Families[0].Id, Description = "Manage information system accounts.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-AC-3", Name = "Access Enforcement", Framework = "NIST 800-53", ControlFamilyId = nist800Families[0].Id, Description = "Enforce approved authorizations.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-AU-2", Name = "Audit Record Content", Framework = "NIST 800-53", ControlFamilyId = nist800Families[1].Id, Description = "Configure audit record content.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-AU-6", Name = "Audit Record Review Analysis and Reporting", Framework = "NIST 800-53", ControlFamilyId = nist800Families[1].Id, Description = "Review and analyze audit records.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-IR-4", Name = "Incident Handling", Framework = "NIST 800-53", ControlFamilyId = nist800Families[2].Id, Description = "Manage incident handling process.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-SC-8", Name = "Transmission Confidentiality and Integrity", Framework = "NIST 800-53", ControlFamilyId = nist800Families[3].Id, Description = "Protect data in transit.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-SI-2", Name = "Flaw Remediation", Framework = "NIST 800-53", ControlFamilyId = nist800Families[4].Id, Description = "Remediate identified flaws in a timely manner.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "NIST-SI-4", Name = "System Monitoring", Framework = "NIST 800-53", ControlFamilyId = nist800Families[4].Id, Description = "Monitor systems for anomalies and threats.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
        };
        db.ComplianceControls.AddRange(nist800Controls);

        var isoControls = new[]
        {
            new ComplianceControl { ControlId = "ISO-A.5.1", Name = "Policies for Information Security", Framework = "ISO 27001", ControlFamilyId = isoFamilies[0].Id, Description = "Establish information security policies.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.6.1", Name = "Organization of Information Security", Framework = "ISO 27001", ControlFamilyId = isoFamilies[0].Id, Description = "Organize information security within the organization.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.7.1", Name = "Human Resource Security", Framework = "ISO 27001", ControlFamilyId = isoFamilies[1].Id, Description = "Ensure personnel are aware of security responsibilities.", Impact = ComplianceImpact.Medium, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.8.1", Name = "User Endpoint Devices", Framework = "ISO 27001", ControlFamilyId = isoFamilies[3].Id, Description = "Secure user endpoint devices.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.8.5", Name = "Secure Authentication", Framework = "ISO 27001", ControlFamilyId = isoFamilies[3].Id, Description = "Implement secure authentication mechanisms.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.8.8", Name = "Management of Technical Vulnerabilities", Framework = "ISO 27001", ControlFamilyId = isoFamilies[3].Id, Description = "Manage technical vulnerabilities in a timely manner.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.8.13", Name = "Information Backup", Framework = "ISO 27001", ControlFamilyId = isoFamilies[3].Id, Description = "Implement information backup policies.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "ISO-A.8.15", Name = "Logging", Framework = "ISO 27001", ControlFamilyId = isoFamilies[3].Id, Description = "Implement logging and monitoring.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
        };
        db.ComplianceControls.AddRange(isoControls);

        var pciControls = new[]
        {
            new ComplianceControl { ControlId = "PCI-REQ1.1", Name = "Install and Maintain Network Security Controls", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[0].Id, Description = "Install firewalls and other network security controls.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ2.1", Name = "Do Not Store Sensitive Authentication Data", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[1].Id, Description = "Do not store sensitive authentication data after authorization.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ3.1", Name = "Protect Stored Account Data", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[1].Id, Description = "Protect stored cardholder data.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ4.1", Name = "Encrypt Transmission of Cardholder Data", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[1].Id, Description = "Encrypt cardholder data in transit.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ5.1", Name = "Protect All Systems Against Malware", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[2].Id, Description = "Implement anti-malware on all systems.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ6.1", Name = "Develop and Maintain Secure Systems", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[2].Id, Description = "Develop and maintain secure systems and applications.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ7.1", Name = "Restrict Access to Cardholder Data", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[3].Id, Description = "Restrict access to cardholder data on a need-to-know basis.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ8.1", Name = "Identify Users and Authenticate Access", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[3].Id, Description = "Identify and authenticate all users.", Impact = ComplianceImpact.Critical, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ10.1", Name = "Log and Monitor Access", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[4].Id, Description = "Log and monitor all access to cardholder data.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ11.1", Name = "Regularly Test Security Systems", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[4].Id, Description = "Regularly test security systems and processes.", Impact = ComplianceImpact.High, CreatedAt = DateTime.UtcNow },
            new ComplianceControl { ControlId = "PCI-REQ12.1", Name = "Maintain Information Security Policy", Framework = "PCI-DSS", ControlFamilyId = pciFamilies[5].Id, Description = "Maintain an information security policy.", Impact = ComplianceImpact.Medium, CreatedAt = DateTime.UtcNow },
        };
        db.ComplianceControls.AddRange(pciControls);
        await db.SaveChangesAsync();
    }
}
