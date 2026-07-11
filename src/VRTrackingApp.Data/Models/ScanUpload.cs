using System;

namespace VRTrackingApp.Data.Models;

public class ScanUpload
{
    public int Id { get; set; }
    public string FileName { get; set; } = default!;
    public string? FileHash { get; set; }
    public long FileSize { get; set; }
    public string Status { get; set; } = "Pending";
    public int? UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}