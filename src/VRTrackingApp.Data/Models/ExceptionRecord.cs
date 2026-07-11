using System;

namespace VRTrackingApp.Data.Models;

public class ExceptionRecord
{
    public int Id { get; set; }
    public int VulnerabilityInstanceId { get; set; }
    public string Reason { get; set; } = default!;
    public int? ApprovedByUserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}