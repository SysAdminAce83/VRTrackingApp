using System;

namespace VRTrackingApp.Data.Models;

public class RemediationAction
{
    public int Id { get; set; }
    public int VulnerabilityInstanceId { get; set; }
    public string Action { get; set; } = default!;
    public string Status { get; set; } = "Open";
    public int? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}