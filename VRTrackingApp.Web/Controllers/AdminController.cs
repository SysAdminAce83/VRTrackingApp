using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VRTrackingApp.Data.Models;
using VRTrackingApp.Web.Services;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly VRTrackingAppContext _db;
    private readonly AuditLogService _audit;
    public AdminController(VRTrackingAppContext db, AuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet("/diagnostic/auditcount")]
    public async Task<IActionResult> AuditCount()
    {
        var c = await _db.AuditLogs.CountAsync();
        return Content($"AuditLogs={c}");
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Admin";
        var users = await _db.UserAccounts.Include(u => u.Role).OrderBy(u => u.DisplayName).ToListAsync();
        var roles = await _db.Roles.ToListAsync();
        ViewBag.Roles = roles;

        var me = AuditLogService.CurrentUserId(User);
        ViewBag.CanDelete = users.Any(u => u.Id != me);
        ViewBag.AuditTrail = await _db.AuditLogs.Include(a => a.PerformedBy)
            .OrderByDescending(a => a.PerformedAt).Take(15).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string userName, string email, string? displayName,
        int roleId, string? location, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Username and email are required.";
            return RedirectToAction("Index");
        }
        if (await _db.UserAccounts.AnyAsync(u => u.UserName == userName))
        {
            TempData["Error"] = $"User '{userName}' already exists.";
            return RedirectToAction("Index");
        }

        var user = new UserAccount
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName ?? userName,
            RoleId = roleId,
            Location = string.IsNullOrWhiteSpace(location) ? null : location,
            IsActive = isActive,
            CreatedByUserId = AuditLogService.CurrentUserId(User)
        };
        _db.UserAccounts.Add(user);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("User", "Created", $"user '{userName}'",
            $"Role={_db.Roles.Find(roleId)?.Name}; Location={location}; Active={isActive}", AuditLogService.CurrentUserId(User));
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string email, string? displayName,
        int roleId, string? location, bool isActive = true)
    {
        var user = await _db.UserAccounts.FindAsync(id);
        if (user == null) return NotFound();

        var changes = new List<string>();
        if (user.Email != email) { changes.Add($"email: {user.Email} -> {email}"); user.Email = email; }
        if (user.DisplayName != displayName) { changes.Add($"display: {user.DisplayName} -> {displayName}"); user.DisplayName = displayName; }
        if (user.RoleId != roleId) { changes.Add($"role: {user.Role?.Name} -> {_db.Roles.Find(roleId)?.Name}"); user.RoleId = roleId; }
        var newLoc = string.IsNullOrWhiteSpace(location) ? null : location;
        if (user.Location != newLoc) { changes.Add($"location: {user.Location} -> {newLoc}"); user.Location = newLoc; }
        if (user.IsActive != isActive) { changes.Add($"active: {user.IsActive} -> {isActive}"); user.IsActive = isActive; }

        await _db.SaveChangesAsync();
        await _audit.LogAsync("User", "Edited", $"user '{user.UserName}'",
            changes.Count == 0 ? "No fields changed." : string.Join("; ", changes), AuditLogService.CurrentUserId(User));
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.UserAccounts.FindAsync(id);
        if (user == null) return NotFound();

        // Prevent an admin from deleting themselves (lockout protection).
        if (id == AuditLogService.CurrentUserId(User))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction("Index");
        }

        _db.UserAccounts.Remove(user);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("User", "Deleted", $"user '{user.UserName}'",
            $"Role={user.Role?.Name}; Location={user.Location}", AuditLogService.CurrentUserId(User));
        return RedirectToAction("Index");
    }
}
