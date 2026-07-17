using System;

namespace VRTrackingApp.Data.Models;

public class RemediationAction
{
    public int Id { get; set; }
    public int VulnerabilityInstanceId { get; set; }
    public VulnerabilityInstance? VulnerabilityInstance { get; set; }

    public string Action { get; set; } = default!;
    public string Status { get; set; } = "Open";
    public int? AssignedToUserId { get; set; }
    public UserAccount? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ExceptionExpiryDate { get; set; }
    public string? Comments { get; set; }
    public string? EvidenceFileName { get; set; }

    public int? PerformedByUserId { get; set; }
    public UserAccount? PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
