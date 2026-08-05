using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class Framework
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string? Version { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ControlFamily> ControlFamilies { get; set; } = new List<ControlFamily>();
    public ICollection<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();
}

public class ControlFamily
{
    public int Id { get; set; }
    public string FamilyId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int FrameworkId { get; set; }
    public Framework Framework { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ComplianceControl> Controls { get; set; } = new List<ComplianceControl>();
}