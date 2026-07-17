using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Audit trail for manual changes made to the asset inventory through the
/// Assets UI (create, edit, delete). Used for accountability / audit purposes.
/// </summary>
public class AssetAuditTrail
{
    public int Id { get; set; }

    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    // Created | Updated | Deleted
    public string Action { get; set; } = default!;

    public int? PerformedByUserId { get; set; }
    public UserAccount? PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Human-readable description of what changed (e.g. "Category: server → Server").
    public string? Details { get; set; }

    public ICollection<AssetFieldChange> FieldChanges { get; set; } = new List<AssetFieldChange>();
}
