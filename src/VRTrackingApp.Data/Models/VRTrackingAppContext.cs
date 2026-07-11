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
    public DbSet<ExceptionRecord> ExceptionRecords { get; set; }
    public DbSet<UploadAuditTrail> UploadAuditTrails { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurations can be added here if needed.
    }
}
