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
}