using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class UserAccount
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RoleId { get; set; }
    public Role? Role { get; set; }
    public string? MfaSecret { get; set; }
    public bool MfaEnabled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Physical site/location the user belongs to (used for location-scoped exception approval).</summary>
    public string? Location { get; set; }

    /// <summary>User who created this account (audit). Null for seeded/initial accounts.</summary>
    public int? CreatedByUserId { get; set; }
    public UserAccount? CreatedBy { get; set; }
}