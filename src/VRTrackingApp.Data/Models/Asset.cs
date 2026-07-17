using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Canonical asset inventory record, modelled on an industry-standard
/// asset / Business Impact Analysis (BIA) template. De-duplicated across
/// uploads so the same host (matched by hostname or IP) is represented once.
/// Scan-specific host rows (<see cref="AssetHost"/>) link back via AssetHost.AssetId.
/// </summary>
public class Asset
{
    public int Id { get; set; }

    // ---- 1. Asset Identification ----
    public string HostName { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? ServerId { get; set; }
    public string? AssetStatus { get; set; }
    public string? Datacenter { get; set; }
    public string? Location { get; set; }
    public string? OperatingSystem { get; set; }

    // ---- 2. BIA Criticality (CIA) ----
    public string? BiaCriticality { get; set; }
    public string? Confidentiality { get; set; }
    public string? Integrity { get; set; }
    public string? Availability { get; set; }
    public string? AssetCriticality { get; set; }

    // ---- 3. Application ----
    public string? Application { get; set; }
    public string? ApplicationOwner { get; set; }
    public string? ActiveDirectoryComments { get; set; }
    public string? Environment { get; set; }
    public string? BackupDocument { get; set; }
    public string? BackupOwner { get; set; }
    public string? GroupLandscape { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? ApplicationSystemName { get; set; }
    public string? SystemDescription { get; set; }

    // ---- 4. Ownership & Points of Contact (POC) ----
    public string? AssetOwner { get; set; }
    public string? BusinessOwner { get; set; }
    public string? InternalPoc { get; set; }
    public string? OnsiteResourceBackup { get; set; }
    public string? PocRole { get; set; }
    public string? ExternalPoc { get; set; }
    public string? ExternalPocRole { get; set; }

    // ---- 5. Hardware Requirements ----
    public int? CpuCoreCount { get; set; }
    public int? RamGb { get; set; }
    public int? DriveCount { get; set; }
    public int? TotalDiskSpaceGb { get; set; }
    public string? Vendor { get; set; }
    public string? HardwareDescription { get; set; }

    // ---- 6. Resources, Software & Redundancy ----
    public string? Hardware { get; set; }
    public string? Software { get; set; }
    public string? ServerName2 { get; set; }
    public string? ServerIpAddress2 { get; set; }
    public string? OtherItResources { get; set; }
    public string? RedundancyPrimaryDc { get; set; }
    public string? CriticalRoles { get; set; }
    public string? CriticalResources { get; set; }

    // ---- 7. Dependencies & Impact ----
    public string? InterDependencies { get; set; }
    public string? DataReplicationFrequency { get; set; }
    public string? PeakTime { get; set; }
    public string? OffPeakTime { get; set; }
    public string? OutageImpact { get; set; }
    public string? FinancialImpact { get; set; }
    public string? NonFinancialImpact { get; set; }
    public string? RegulatoryImpact { get; set; }
    public string? Criticality2 { get; set; }

    // ---- 8. Disaster Recovery & Business Continuity ----
    public string? DisasterRecoveryRequired { get; set; }
    public string? DrSetupDetails { get; set; }
    public string? RedundancyDrDc { get; set; }
    public string? PrioritizeResourceRecovery { get; set; }
    public string? MinimumHardware { get; set; }
    public string? MinimumHardware2 { get; set; }
    public string? MinimumItResources { get; set; }
    public string? Rto { get; set; }
    public string? Rpo { get; set; }
    public string? Mol { get; set; }
    public string? Remarks { get; set; }
    public string? BackupSchedule { get; set; }
    public string? BusUnit { get; set; }

    // ---- Free-text notes ----
    public string? Notes { get; set; }

    // ---- Audit / lineage ----
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetHost> ScanHosts { get; set; } = new List<AssetHost>();
}
