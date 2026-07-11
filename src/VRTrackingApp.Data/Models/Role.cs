using System;
using System.Collections.Generic;

namespace VRTrackingApp.Data.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public ICollection<UserAccount> Users { get; set; } = new List<UserAccount>();
}