using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Notifications;

/// <summary>Resolves the application users that should be notified for a given role.</summary>
public class UserNotificationService
{
    private readonly VRTrackingAppContext _db;
    public UserNotificationService(VRTrackingAppContext db) => _db = db;

    public async Task<List<UserAccount>> UsersInRoleAsync(string role)
    {
        return await _db.UserAccounts
            .Include(u => u.Role)
            .Where(u => u.IsActive && u.Role != null && u.Role.Name == role)
            .ToListAsync();
    }

    public async Task<List<UserAccount>> UsersInRolesAsync(IEnumerable<string> roles)
    {
        var set = roles.ToHashSet();
        return await _db.UserAccounts
            .Include(u => u.Role)
            .Where(u => u.IsActive && u.Role != null && set.Contains(u.Role.Name))
            .ToListAsync();
    }
}
