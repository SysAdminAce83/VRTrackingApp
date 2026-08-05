using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class ComplianceControl
{
    public int Id { get; set; }
    public string ControlId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Framework { get; set; } = default!;
    public string? FrameworkVersion { get; set; }
    public int? ControlFamilyId { get; set; }
    public ControlFamily? ControlFamilyNavigation { get; set; }
    public string Description { get; set; } = default!;
    public ComplianceImpact Impact { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<FindingComplianceLink> FindingLinks { get; set; } = new List<FindingComplianceLink>();
    public ICollection<ControlEvidence> Evidence { get; set; } = new List<ControlEvidence>();
}

public enum ComplianceImpact
{
    None,
    Low,
    Medium,
    High,
    Critical
}