using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Domain.Models;

namespace VRTrackingApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Scan> Scans { get; set; } = default!;
        public DbSet<Asset> Assets { get; set; } = default!;
        public DbSet<Vulnerability> Vulnerabilities { get; set; } = default!;
        public DbSet<AssetVulnerability> AssetVulnerabilities { get; set; } = default!;
        public DbSet<Reference> References { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Scan entity
            modelBuilder.Entity<Scan>(entity =>
            {
                entity.ToTable("Scans");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScanName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ScanType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileHash).HasMaxLength(64); // SHA-256
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Processing");
                entity.HasIndex(e => e.ScanDate);
                entity.HasMany(e => e.Assets)
                    .WithOne(a => a.Scan)
                    .HasForeignKey(a => a.ScanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Asset entity
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.ToTable("Assets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IPAddress).IsRequired().HasMaxLength(45); // IPv6 max length
                entity.Property(e => e.HostName).HasMaxLength(255);
                entity.Property(e => e.MACAddress).HasMaxLength(17); // AA:BB:CC:DD:EE:FF
                entity.Property(e => e.NetBIOSName).HasMaxLength(255);
                entity.Property(e => e.DNSName).HasMaxLength(255);
                entity.Property(e => e.OperatingSystem).HasMaxLength(255);
                entity.Property(e => e.OSVersion).HasMaxLength(100);
                entity.HasIndex(e => e.ScanId);
                entity.HasIndex(e => e.IPAddress);
                entity.HasMany(e => e.AssetVulnerabilities)
                    .WithOne(av => av.Asset)
                    .HasForeignKey(av => av.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Vulnerability entity
            modelBuilder.Entity<Vulnerability>(entity =>
            {
                entity.ToTable("Vulnerabilities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PluginID).IsRequired();
                entity.Property(e => e.PluginName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CVE).HasMaxLength(20);
                entity.Property(e => e.CVSSVector).HasMaxLength(100);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.PluginID);
                entity.HasIndex(e => e.CVE);
                entity.HasIndex(e => e.Severity);
                entity.HasMany(e => e.AssetVulnerabilities)
                    .WithOne(av => av.Vulnerability)
                    .HasForeignKey(av => av.VulnerabilityId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.References)
                    .WithOne(r => r.Vulnerability)
                    .HasForeignKey(r => r.VulnerabilityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AssetVulnerability entity
            modelBuilder.Entity<AssetVulnerability>(entity =>
            {
                entity.ToTable("AssetVulnerabilities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.VulnerabilityId);
                entity.HasIndex(e => new { e.AssetId, e.VulnerabilityId, e.Port, e.Protocol }).IsUnique();
            });

            // Configure Reference entity
            modelBuilder.Entity<Reference>(entity =>
            {
                entity.ToTable("References");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReferenceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ReferenceValue).IsRequired().HasMaxLength(500);
                entity.Property(e => e.URL).HasMaxLength(2048);
                entity.HasIndex(e => e.VulnerabilityId);
            });
        }
    }
}