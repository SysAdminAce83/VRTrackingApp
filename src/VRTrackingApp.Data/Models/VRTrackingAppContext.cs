using System;
using Microsoft.EntityFrameworkCore;

namespace VRTrackingApp.Data.Models;

public class VRTrackingAppContext : DbContext
{
    public VRTrackingAppContext(DbContextOptions<VRTrackingAppContext> options)
        : base(options) { }

    public DbSet<ScanUpload> ScanUploads { get; set; }
    public DbSet<AssetHost> AssetHosts { get; set; }
    public DbSet<VulnerabilityFinding> VulnerabilityFindings { get; set; }
    public DbSet<VulnerabilityInstance> VulnerabilityInstances { get; set; }
    public DbSet<RemediationAction> RemediationActions { get; set; }
    public DbSet<RemediationJob> RemediationJobs { get; set; }
    public DbSet<ExceptionRecord> ExceptionRecords { get; set; }
    public DbSet<UploadAuditTrail> UploadAuditTrails { get; set; }
    public DbSet<ScanGroup> ScanGroups { get; set; }
    public DbSet<ScanMetadata> ScanMetadatas { get; set; }
    public DbSet<IngestionAudit> IngestionAudits { get; set; }
    public DbSet<DeduplicationLog> DeduplicationLogs { get; set; }
    public DbSet<ScanIngestionLock> ScanIngestionLocks { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAuditTrail> AssetAuditTrails { get; set; }
    public DbSet<AssetFieldChange> AssetFieldChanges { get; set; }
public DbSet<AppSetting> AppSettings { get; set; }

    // Exception module V2
    public DbSet<ExceptionMitigation> ExceptionMitigations { get; set; }
    public DbSet<ExceptionEvidence> ExceptionEvidence { get; set; }
    public DbSet<ExceptionSecurityControl> ExceptionSecurityControls { get; set; }
    public DbSet<ExceptionApprovalStep> ExceptionApprovalSteps { get; set; }
    public DbSet<ExceptionReviewHistory> ExceptionReviewHistories { get; set; }
    public DbSet<ExceptionComment> ExceptionComments { get; set; }
    public DbSet<VendorResponse> VendorResponses { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<TicketingLink> TicketingLinks { get; set; }

    // Compliance / GRC
    public DbSet<Framework> Frameworks { get; set; }
    public DbSet<ControlFamily> ControlFamilies { get; set; }
    public DbSet<ComplianceControl> ComplianceControls { get; set; }
    public DbSet<FindingComplianceLink> FindingComplianceLinks { get; set; }
    public DbSet<ComplianceReview> ComplianceReviews { get; set; }
    public DbSet<RiskAcceptance> RiskAcceptances { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Standard> Standards { get; set; }
    public DbSet<Procedure> Procedures { get; set; }
    public DbSet<Risk> Risks { get; set; }
    public DbSet<ControlEvidence> ControlEvidences { get; set; }
    public DbSet<EvidenceAttachment> EvidenceAttachments { get; set; }
    public DbSet<ControlLibrary> ControlLibraries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScanUpload>(e =>
        {
            e.HasIndex(s => s.ScanDate);
            e.HasIndex(s => s.SourceType);
            e.HasIndex(s => s.FileHash);
            e.HasIndex(s => s.ScanGroupId);
            e.HasIndex(s => s.Format);
            e.Property(s => s.FileName).HasMaxLength(255).IsRequired();
            e.Property(s => s.FileHash).HasMaxLength(64);
            e.Property(s => s.Md5Hash).HasMaxLength(32);
            e.Property(s => s.Format).HasMaxLength(20);

            e.HasOne(s => s.ScanGroup)
                .WithMany(g => g.Uploads)
                .HasForeignKey(s => s.ScanGroupId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(s => s.Metadata)
                .WithOne(m => m.ScanUpload)
                .HasForeignKey<ScanMetadata>(m => m.ScanUploadId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.IngestionAudit)
                .WithOne(a => a.ScanUpload)
                .HasForeignKey<IngestionAudit>(a => a.ScanUploadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScanGroup>(e =>
        {
            e.HasIndex(g => g.ScanKey).IsUnique();
            e.HasIndex(g => g.NessusScanUuid);
            e.HasIndex(g => g.IngestState);
            e.Property(g => g.ScanKey).HasMaxLength(64).IsRequired();
            e.Property(g => g.NessusScanUuid).HasMaxLength(64);
            e.Property(g => g.ScannerName).HasMaxLength(255);
            e.Property(g => g.PolicyName).HasMaxLength(255);
            e.Property(g => g.PolicyId).HasMaxLength(64);
            e.Property(g => g.SourceType).HasMaxLength(50);
            e.Property(g => g.ScanCycleLabel).HasMaxLength(255);
        });

        modelBuilder.Entity<ScanMetadata>(e =>
        {
            e.HasIndex(m => m.ScanUploadId).IsUnique();
            e.Property(m => m.NessusScanUuid).HasMaxLength(64);
            e.Property(m => m.ScannerName).HasMaxLength(255);
            e.Property(m => m.PolicyName).HasMaxLength(255);
            e.Property(m => m.PolicyId).HasMaxLength(64);
            e.Property(m => m.ScanTarget).HasMaxLength(255);
        });

        modelBuilder.Entity<IngestionAudit>(e =>
        {
            e.HasIndex(a => a.ScanUploadId).IsUnique();
            e.HasIndex(a => a.ScanGroupId);
            e.HasIndex(a => a.PerformedAt);
            e.Property(a => a.Outcome).HasMaxLength(20).IsRequired();
            e.Property(a => a.DuplicateStatus).HasMaxLength(20);
            e.Property(a => a.Reason).HasMaxLength(2000);
            e.Property(a => a.ProcessingLog).HasMaxLength(8000);
        });

        modelBuilder.Entity<DeduplicationLog>(e =>
        {
            e.HasIndex(d => d.ScanUploadId);
            e.HasIndex(d => d.VulnerabilityKey);
            e.HasIndex(d => new { d.PluginId, d.HostName, d.Port, d.Protocol });
            e.Property(d => d.VulnerabilityKey).HasMaxLength(128).IsRequired();
            e.Property(d => d.HostName).HasMaxLength(255);
            e.Property(d => d.IpAddress).HasMaxLength(45);
            e.Property(d => d.Cve).HasMaxLength(40);
            e.Property(d => d.Protocol).HasMaxLength(10);
            e.Property(d => d.Decision).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<ScanIngestionLock>(e =>
        {
            e.HasIndex(l => l.ScanGroupId).IsUnique();
            e.HasIndex(l => l.State);
            e.Property(l => l.State).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<AssetHost>(e =>
        {
            e.HasIndex(a => a.ScanUploadId);
            e.HasIndex(a => a.IpAddress);
            e.HasIndex(a => a.AssetId);
            e.Property(a => a.IpAddress).HasMaxLength(45).IsRequired();
            e.Property(a => a.HostName).HasMaxLength(255);

            e.HasOne(a => a.Asset)
                .WithMany(x => x.ScanHosts)
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasIndex(a => a.HostName);
            e.HasIndex(a => a.IpAddress);
            e.HasIndex(a => a.AssetStatus);
            e.HasIndex(a => a.Datacenter);
            e.HasIndex(a => a.Environment);
            e.HasIndex(a => a.Category);
            e.Property(a => a.HostName).HasMaxLength(255).IsRequired();
            e.Property(a => a.IpAddress).HasMaxLength(45).IsRequired();
            e.Property(a => a.OperatingSystem).HasMaxLength(255);
            e.Property(a => a.Location).HasMaxLength(255);
            e.Property(a => a.Category).HasMaxLength(100);
            e.Property(a => a.SubCategory).HasMaxLength(100);
            e.Property(a => a.Notes).HasMaxLength(4000);
        });

        modelBuilder.Entity<VulnerabilityFinding>(e =>
        {
            e.HasIndex(v => v.PluginId);
            e.HasIndex(v => v.Severity);
            e.Property(v => v.PluginName).HasMaxLength(255).IsRequired();
            e.Property(v => v.Severity).HasMaxLength(20).IsRequired();
            e.Property(v => v.Cve).HasMaxLength(20);
        });

        modelBuilder.Entity<VulnerabilityInstance>(e =>
        {
            e.HasIndex(i => i.AssetHostId);
            e.HasIndex(i => i.VulnerabilityFindingId);
            e.HasIndex(i => i.Status);
            e.Property(i => i.Status).HasMaxLength(20).IsRequired();
            e.Property(i => i.Protocol).HasMaxLength(10);
        });

        modelBuilder.Entity<RemediationAction>(e =>
        {
            e.HasIndex(r => r.VulnerabilityInstanceId);
            e.Property(r => r.Action).HasMaxLength(50).IsRequired();
            e.Property(r => r.Status).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<RemediationJob>(e =>
        {
            e.HasIndex(j => j.VulnerabilityInstanceId);
            e.HasIndex(j => j.State);
            e.Property(j => j.JobType).HasMaxLength(20).IsRequired();
            e.Property(j => j.State).HasMaxLength(20).IsRequired();
            e.Property(j => j.TargetHost).HasMaxLength(255);
            e.Property(j => j.OperatingSystem).HasMaxLength(255);
            e.Property(j => j.PatchId).HasMaxLength(50);
            e.Property(j => j.ResultSummary).HasMaxLength(1000);
            e.HasOne(j => j.VulnerabilityInstance)
                .WithMany()
                .HasForeignKey(j => j.VulnerabilityInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(j => j.RequestedBy)
                .WithMany()
                .HasForeignKey(j => j.RequestedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExceptionRecord>(e =>
        {
            e.HasIndex(x => x.VulnerabilityInstanceId);
            e.HasIndex(x => x.State);
            e.HasIndex(x => x.OwnerUserId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
            e.Property(x => x.PendingReason).HasMaxLength(2000);
            e.Property(x => x.RejectionReason).HasMaxLength(2000);
            e.Property(x => x.Stage1Role).HasMaxLength(50);
            e.Property(x => x.OtherReasonText).HasMaxLength(2000);
            e.Property(x => x.TechnicalJustification).HasMaxLength(4000);
            e.Property(x => x.DowntimeConstraint).HasMaxLength(2000);
            e.Property(x => x.BusinessImpact).HasMaxLength(2000);
            e.Property(x => x.CostImpact).HasMaxLength(2000);
            e.Property(x => x.ProductionImpact).HasMaxLength(2000);
            e.Property(x => x.CustomerImpact).HasMaxLength(2000);
            e.Property(x => x.ComplianceImpact).HasMaxLength(2000);
            // enum -> string for readable storage
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            e.Property(x => x.CurrentApprovalStage).HasConversion<string>().HasMaxLength(40);
            e.Property(x => x.NonFixableReason).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Likelihood).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Impact).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.OverallRisk).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Exploitability).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.InternetExposure).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.ClosedReason).HasConversion<string>().HasMaxLength(30);
            e.HasOne(x => x.Owner)
                .WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ActionedBy)
                .WithMany().HasForeignKey(x => x.ActionedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExceptionMitigation>(e =>
        {
            e.HasIndex(m => m.ExceptionRecordId);
            e.Property(m => m.Description).HasMaxLength(1000).IsRequired();
            e.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.ExceptionRecord)
                .WithMany(x => x.Mitigations)
                .HasForeignKey(m => m.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExceptionEvidence>(e =>
        {
            e.HasIndex(v => v.ExceptionRecordId);
            e.Property(v => v.EvidenceType).HasConversion<string>().HasMaxLength(40);
            e.Property(v => v.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(v => v.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(v => v.ContentHash).HasMaxLength(128);
            e.HasOne(v => v.ExceptionRecord)
                .WithMany(x => x.Evidence)
                .HasForeignKey(v => v.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.UploadedBy)
                .WithMany().HasForeignKey(v => v.UploadedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExceptionSecurityControl>(e =>
        {
            e.HasIndex(c => c.ExceptionRecordId);
            e.Property(c => c.ControlName).HasMaxLength(100).IsRequired();
            e.HasOne(c => c.ExceptionRecord)
                .WithMany(x => x.SecurityControls)
                .HasForeignKey(c => c.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExceptionApprovalStep>(e =>
        {
            e.HasIndex(s => s.ExceptionRecordId);
            e.Property(s => s.RequiredRole).HasMaxLength(50).IsRequired();
            e.Property(s => s.Stage).HasConversion<string>().HasMaxLength(30);
            e.Property(s => s.Decision).HasConversion<string>().HasMaxLength(20);
            e.Property(s => s.Comment).HasMaxLength(2000);
            e.HasOne(s => s.ExceptionRecord)
                .WithMany(x => x.ApprovalSteps)
                .HasForeignKey(s => s.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.DecisionBy)
                .WithMany().HasForeignKey(s => s.DecisionByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExceptionReviewHistory>(e =>
        {
            e.HasIndex(r => r.ExceptionRecordId);
            e.Property(r => r.Outcome).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Comment).HasMaxLength(2000);
            e.HasOne(r => r.ExceptionRecord)
                .WithMany(x => x.Reviews)
                .HasForeignKey(r => r.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.ReviewedBy)
                .WithMany().HasForeignKey(r => r.ReviewedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ExceptionComment>(e =>
        {
            e.HasIndex(c => c.ExceptionRecordId);
            e.Property(c => c.Body).HasMaxLength(4000).IsRequired();
            e.Property(c => c.AuthorDisplayName).HasMaxLength(150);
            e.HasOne(c => c.ExceptionRecord)
                .WithMany(x => x.Comments)
                .HasForeignKey(c => c.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User)
                .WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VendorResponse>(e =>
        {
            e.HasIndex(v => v.ExceptionRecordId);
            e.Property(v => v.Vendor).HasMaxLength(150);
            e.Property(v => v.ResponseText).HasMaxLength(4000);
            e.HasOne(v => v.ExceptionRecord)
                .WithMany(x => x.VendorResponses)
                .HasForeignKey(v => v.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasIndex(n => n.UserId);
            e.HasIndex(n => n.IsRead);
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(40);
            e.Property(n => n.Title).HasMaxLength(255).IsRequired();
            e.Property(n => n.Message).HasMaxLength(2000);
            e.HasOne(n => n.User)
                .WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.ExceptionRecord)
                .WithMany().HasForeignKey(n => n.ExceptionRecordId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketingLink>(e =>
        {
            e.HasIndex(t => t.ExceptionRecordId);
            e.Property(t => t.System).HasMaxLength(40).IsRequired();
            e.Property(t => t.TicketId).HasMaxLength(80).IsRequired();
            e.Property(t => t.TicketUrl).HasMaxLength(500);
            e.Property(t => t.Title).HasMaxLength(255);
            e.HasOne(t => t.ExceptionRecord)
                .WithMany(x => x.TicketingLinks)
                .HasForeignKey(t => t.ExceptionRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.LinkedBy)
                .WithMany().HasForeignKey(t => t.LinkedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UploadAuditTrail>(e =>
        {
            e.HasIndex(t => t.ScanUploadId);
            e.Property(t => t.Action).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserAccount>(e =>
        {
            e.HasIndex(u => u.UserName);
            e.Property(u => u.UserName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.Location).HasMaxLength(100);
            e.HasOne(u => u.CreatedBy)
                .WithMany().HasForeignKey(u => u.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.PerformedAt);
            e.HasIndex(a => a.Category);
            e.Property(a => a.Category).HasMaxLength(50).IsRequired();
            e.Property(a => a.Action).HasMaxLength(50).IsRequired();
            e.Property(a => a.Target).HasMaxLength(255);
            e.Property(a => a.Detail).HasMaxLength(2000);
            e.Property(a => a.OldValue).HasMaxLength(2000);
            e.Property(a => a.NewValue).HasMaxLength(2000);
            e.Property(a => a.IpAddress).HasMaxLength(64);
            e.HasOne(a => a.PerformedBy)
                .WithMany().HasForeignKey(a => a.PerformedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.Property(r => r.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<AssetAuditTrail>(e =>
        {
            e.HasIndex(t => t.AssetId);
            e.HasIndex(t => t.PerformedAt);
            e.Property(t => t.Action).HasMaxLength(50).IsRequired();
            e.HasOne(t => t.Asset)
                .WithMany()
                .HasForeignKey(t => t.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.PerformedBy)
                .WithMany()
                .HasForeignKey(t => t.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

modelBuilder.Entity<AssetFieldChange>(e =>
        {
            e.HasIndex(c => c.AssetAuditTrailId);
            e.Property(c => c.Field).HasMaxLength(100).IsRequired();
            e.Property(c => c.OldValue).HasMaxLength(2000);
            e.Property(c => c.NewValue).HasMaxLength(2000);
            e.HasOne(c => c.AssetAuditTrail)
                .WithMany(t => t.FieldChanges)
                .HasForeignKey(c => c.AssetAuditTrailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Compliance / GRC ----

        modelBuilder.Entity<Framework>(e =>
        {
            e.HasIndex(f => f.ShortName).IsUnique();
            e.Property(f => f.Name).HasMaxLength(255).IsRequired();
            e.Property(f => f.ShortName).HasMaxLength(50).IsRequired();
            e.Property(f => f.Version).HasMaxLength(20);
            e.Property(f => f.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<ControlFamily>(e =>
        {
            e.HasIndex(cf => cf.FamilyId);
            e.HasIndex(cf => cf.FrameworkId);
            e.Property(cf => cf.FamilyId).HasMaxLength(50).IsRequired();
            e.Property(cf => cf.Name).HasMaxLength(255).IsRequired();
            e.Property(cf => cf.Description).HasMaxLength(4000);
            e.HasOne(cf => cf.Framework)
                .WithMany(f => f.ControlFamilies)
                .HasForeignKey(cf => cf.FrameworkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplianceControl>(e =>
        {
            e.HasIndex(cc => cc.ControlId);
            e.HasIndex(cc => cc.Framework);
            e.HasIndex(cc => cc.Framework);
            e.Property(cc => cc.ControlId).HasMaxLength(50).IsRequired();
            e.Property(cc => cc.Name).HasMaxLength(255).IsRequired();
            e.Property(cc => cc.Framework).HasMaxLength(100).IsRequired();
            e.Property(cc => cc.FrameworkVersion).HasMaxLength(20);
            e.Property(cc => cc.Description).HasMaxLength(4000);
            e.HasOne(cc => cc.ControlFamilyNavigation)
                .WithMany()
                .HasForeignKey(cc => cc.ControlFamilyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FindingComplianceLink>(e =>
        {
            e.HasIndex(fcl => fcl.VulnerabilityFindingId);
            e.HasIndex(fcl => fcl.ComplianceControlId);
            e.HasIndex(fcl => new { fcl.VulnerabilityFindingId, fcl.ComplianceControlId }).IsUnique();
            e.Property(fcl => fcl.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(fcl => fcl.Rationale).HasMaxLength(4000);
            e.Property(fcl => fcl.EvidenceRef).HasMaxLength(500);
            e.HasOne(fcl => fcl.Finding)
                .WithMany()
                .HasForeignKey(fcl => fcl.VulnerabilityFindingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(fcl => fcl.Control)
                .WithMany(c => c.FindingLinks)
                .HasForeignKey(fcl => fcl.ComplianceControlId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplianceReview>(e =>
        {
            e.HasIndex(cr => cr.VulnerabilityFindingId);
            e.HasIndex(cr => cr.ComplianceControlId);
            e.Property(cr => cr.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(cr => cr.Rationale).HasMaxLength(4000);
            e.Property(cr => cr.EvidenceRef).HasMaxLength(500);
            e.Property(cr => cr.ReviewerNotes).HasMaxLength(4000);
            e.Property(cr => cr.ReviewedBy).HasMaxLength(255);
            e.HasOne(cr => cr.Finding)
                .WithMany()
                .HasForeignKey(cr => cr.VulnerabilityFindingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cr => cr.Control)
                .WithMany()
                .HasForeignKey(cr => cr.ComplianceControlId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RiskAcceptance>(e =>
        {
            e.HasIndex(ra => ra.VulnerabilityFindingId);
            e.HasIndex(ra => ra.ComplianceControlId);
            e.Property(ra => ra.Justification).HasMaxLength(4000).IsRequired();
            e.Property(ra => ra.AcceptedBy).HasMaxLength(255).IsRequired();
            e.Property(ra => ra.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(ra => ra.ApprovalNotes).HasMaxLength(4000);
            e.HasOne(ra => ra.Finding)
                .WithMany()
                .HasForeignKey(ra => ra.VulnerabilityFindingId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ra => ra.Control)
                .WithMany()
                .HasForeignKey(ra => ra.ComplianceControlId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Risk>(e =>
        {
            e.HasIndex(r => r.RiskName).IsUnique();
            e.Property(r => r.RiskName).HasMaxLength(200).IsRequired();
            e.Property(r => r.Description).IsRequired();
            e.Property(r => r.BusinessImpact).HasMaxLength(50).IsRequired();
            e.Property(r => r.Likelihood).HasMaxLength(50).IsRequired();
            e.Property(r => r.RiskScore).IsRequired();
            e.HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(r => r.ReviewDate);
            e.Property(r => r.Status).HasMaxLength(50).IsRequired();
            e.Property(r => r.Notes).HasMaxLength(4000);
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(r => r.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<ControlEvidence>(e =>
        {
            e.HasIndex(ce => ce.ComplianceControlId);
            e.HasIndex(ce => ce.FindingComplianceLinkId);
            e.Property(ce => ce.Description).HasMaxLength(4000).IsRequired();
            e.Property(ce => ce.FilePath).HasMaxLength(500);
            e.Property(ce => ce.FileName).HasMaxLength(255);
            e.Property(ce => ce.FileHash).HasMaxLength(128);
            e.HasOne(ce => ce.Control)
                .WithMany(c => c.Evidence)
                .HasForeignKey(ce => ce.ComplianceControlId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ce => ce.FindingLink)
                .WithMany()
                .HasForeignKey(ce => ce.FindingComplianceLinkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EvidenceAttachment>(e =>
        {
            e.HasIndex(ea => ea.ControlEvidenceId);
            e.Property(ea => ea.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(ea => ea.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(ea => ea.ContentType).HasMaxLength(100).IsRequired();
            e.Property(ea => ea.FileHash).HasMaxLength(128).IsRequired();
            e.Property(ea => ea.UploadedBy).HasMaxLength(255).IsRequired();
            e.HasOne(ea => ea.Evidence)
                .WithMany()
                .HasForeignKey(ea => ea.ControlEvidenceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Policy>(e => {
            e.ToTable("Policies");
            e.HasIndex(p => p.Title).IsUnique(false);
            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).IsRequired();
            e.Property(p => p.Category).HasMaxLength(100).IsRequired();
            e.Property(p => p.Version).IsRequired();
            e.Property(p => p.EffectiveDate).IsRequired();
            e.Property(p => p.ReviewDate).IsRequired();
            e.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(p => p.Status).HasMaxLength(50).IsRequired();
            e.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Standard>(e => {
            e.ToTable("Standards");
            e.HasIndex(s => s.Title).IsUnique(false);
            e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            e.Property(s => s.Description).IsRequired();
            e.Property(s => s.Version).IsRequired();
            e.Property(s => s.EffectiveDate).IsRequired();
            e.Property(s => s.ReviewDate).IsRequired();
            e.HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(s => s.Policy)
                .WithMany()
                .HasForeignKey(s => s.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(s => s.Status).HasMaxLength(50).IsRequired();
            e.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Procedure>(e => {
            e.ToTable("Procedures");
            e.HasIndex(p => p.Title).IsUnique(false);
            e.Property(p => p.Title).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).IsRequired();
            e.Property(p => p.Version).IsRequired();
            e.Property(p => p.EffectiveDate).IsRequired();
            e.Property(p => p.ReviewDate).IsRequired();
            e.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Standard)
                .WithMany()
                .HasForeignKey(p => p.StandardId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Status).HasMaxLength(50).IsRequired();
            e.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<ControlLibrary>(e => {
            e.ToTable("ControlLibraries");
            e.HasIndex(c => c.ControlId).IsUnique();
            e.Property(c => c.ControlId).HasMaxLength(50).IsRequired();
            e.Property(c => c.Domain).HasMaxLength(100).IsRequired();
            e.Property(c => c.ControlName).HasMaxLength(200).IsRequired();
            e.Property(c => c.ControlDescription).IsRequired();
            e.Property(c => c.ControlOwner).HasMaxLength(100);
            e.Property(c => c.Frequency).HasMaxLength(50);
            e.Property(c => c.Evidence).HasMaxLength(500);
            e.Property(c => c.TestSteps).HasMaxLength(1000);
            e.Property(c => c.RiskAddressed).HasMaxLength(200);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}



