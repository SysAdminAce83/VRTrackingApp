using System;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// A single field-level change captured for an <see cref="AssetAuditTrail"/> entry.
/// Captures the old and new value so every edit is fully auditable.
/// </summary>
public class AssetFieldChange
{
    public int Id { get; set; }

    public int AssetAuditTrailId { get; set; }
    public AssetAuditTrail? AssetAuditTrail { get; set; }

    public string Field { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
