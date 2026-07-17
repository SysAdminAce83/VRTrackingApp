using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services;

/// <summary>
/// After Windows/Active Directory authenticates the user (Single Sign-On), this
/// maps the domain account to the application's <see cref="UserAccount"/> record so
/// the correct role is applied. Access control stays in the database:
///   - user found &amp; active  -> role claim + "enrolled" marker claim are added
///   - user not found          -> no marker claim -> the fallback authorization
///                                policy denies access (Access Denied page)
///
/// In demo mode (EF Core InMemory) an unknown domain user is granted the Admin
/// role so the app can be explored without pre-provisioning accounts.
/// </summary>
public class DomainRoleClaimsTransformation : IClaimsTransformation
{
    public const string EnrolledClaimType = "vrapp:enrolled";

    private readonly VRTrackingAppContext _db;
    private readonly bool _isDemoMode;

    public DomainRoleClaimsTransformation(VRTrackingAppContext db, IConfiguration config)
    {
        _db = db;
        _isDemoMode = config.GetValue("ConnectionStrings:UseInMemory", "true") != "false";
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;

        // Nothing to do for anonymous requests, or if we've already enrolled them.
        if (identity is null || identity.IsAuthenticated != true)
            return principal;
        if (principal.HasClaim(c => c.Type == EnrolledClaimType))
            return principal;

        var samAccountName = ExtractSamAccountName(identity.Name);
        if (string.IsNullOrWhiteSpace(samAccountName))
            return principal;

        var user = await _db.UserAccounts
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.IsActive && u.UserName.ToLower() == samAccountName.ToLower());

        if (user is not null)
        {
            AddClaims(identity, user.Id.ToString(), user.DisplayName ?? user.UserName,
                user.Role?.Name ?? "Analyst", user.Email);
        }
        else if (_isDemoMode)
        {
            // Demo/evaluation convenience only - never happens against SQL Server.
            AddClaims(identity, "0", identity.Name ?? samAccountName, "Admin", null);
        }
        // else: production and not enrolled -> leave without marker claim -> denied.

        return principal;
    }

    private static void AddClaims(ClaimsIdentity identity, string userId, string displayName,
        string role, string? email)
    {
        // Friendly display name for the top bar (Windows gives us DOMAIN\user).
        if (!identity.HasClaim(c => c.Type == ClaimTypes.GivenName))
            identity.AddClaim(new Claim(ClaimTypes.GivenName, displayName));

        if (!identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

        if (!identity.HasClaim(c => c.Type == ClaimTypes.Role))
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

        if (!string.IsNullOrWhiteSpace(email) && !identity.HasClaim(c => c.Type == ClaimTypes.Email))
            identity.AddClaim(new Claim(ClaimTypes.Email, email));

        identity.AddClaim(new Claim(EnrolledClaimType, "true"));
    }

    /// <summary>
    /// Windows identities arrive as "DOMAIN\\username" (or occasionally
    /// "username@domain"). The database stores the bare sAMAccountName.
    /// </summary>
    private static string ExtractSamAccountName(string? windowsName)
    {
        if (string.IsNullOrWhiteSpace(windowsName))
            return string.Empty;

        var backslash = windowsName.IndexOf('\\');
        if (backslash >= 0)
            return windowsName[(backslash + 1)..];

        var at = windowsName.IndexOf('@');
        if (at >= 0)
            return windowsName[..at];

        return windowsName;
    }
}
