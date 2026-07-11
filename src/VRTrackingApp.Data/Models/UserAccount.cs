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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}