using System;

namespace VRTrackingApp.Data.Models;

public class UploadAuditTrail
{
    public int Id { get; set; }
    public int ScanUploadId { get; set; }
    public string Action { get; set; } = default!;
    public int? PerformedByUserId { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}