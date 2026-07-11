using System;
using System.Collections.Generic;

namespace VRTrackingApp.Domain.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public int ScanId { get; set; }
        public string? HostName { get; set; }
        public string IPAddress { get; set; } = default!;
        public string? MACAddress { get; set; }
        public string? NetBIOSName { get; set; }
        public string? DNSName { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OSVersion { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Scan Scan { get; set; } = default!;
        public ICollection<AssetVulnerability> AssetVulnerabilities { get; set; } = new List<AssetVulnerability>();
    }
}