using System;

namespace VRTrackingApp.Data.Models;

public class AssetHost
{
    public int Id { get; set; }
    public int ScanUploadId { get; set; }
    public string? HostName { get; set; }
    public string IpAddress { get; set; } = default!;
    public string? OperatingSystem { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ScanUpload? ScanUpload { get; set; }
    public ICollection<VulnerabilityInstance> Instances { get; set; } = new List<VulnerabilityInstance>();

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }
}