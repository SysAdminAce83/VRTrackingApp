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
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAuditTrail> AssetAuditTrails { get; set; }
    public DbSet<AssetFieldChange> AssetFieldChanges { get; set; }

    // Exception module V2
    public DbSet<ExceptionMitigation> ExceptionMitigations { get; set; }
    public DbSet<ExceptionEvidence> ExceptionEvidence { get; set; }
    public DbSet<ExceptionSecurityControl> ExceptionSecurityControls { get; set; }
    public DbSet<ExceptionApprovalStep> ExceptionApprovalSteps { get; set; }
    public DbSet<ExceptionReviewHistory> ExceptionReviewHistories { get; set; }
    public DbSet<ExceptionComment> ExceptionComments { get; set; }
    public DbSet<VendorResponse> VendorResponses { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScanUpload>(e =>
        {
            e.HasIndex(s => s.ScanDate);
            e.HasIndex(s => s.SourceType);
            e.Property(s => s.FileName).HasMaxLength(255).IsRequired();
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
    }
}
