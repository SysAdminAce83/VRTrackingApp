using System;

namespace VRTrackingApp.Data.Models;

/// <summary>
/// Append-only audit trail of significant changes made through the app
/// (user management, exception workflow, etc.). Never updated or deleted.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    /// <summary>User who performed the action (null if system/anonymous).</summary>
    public int? PerformedByUserId { get; set; }
    public UserAccount? PerformedBy { get; set; }

    public string? PerformedByDisplayName { get; set; }

    /// <summary>Category, e.g. "User", "Exception", "Role".</summary>
    public string Category { get; set; } = "System";

    /// <summary>Action verb, e.g. "Created", "Edited", "Deleted", "Approved", "Rejected".</summary>
    public string Action { get; set; } = "";

    /// <summary>Human-readable target, e.g. "user 'bjones'" or "exception #12 (WEB-01)".</summary>
    public string? Target { get; set; }

    /// <summary>Free-text description of what changed.</summary>
    public string? Detail { get; set; }

    /// <summary>Previous value (for field-level change auditing).</summary>
    public string? OldValue { get; set; }

    /// <summary>New value (for field-level change auditing).</summary>
    public string? NewValue { get; set; }

    /// <summary>Client IP address that performed the action.</summary>
    public string? IpAddress { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}
