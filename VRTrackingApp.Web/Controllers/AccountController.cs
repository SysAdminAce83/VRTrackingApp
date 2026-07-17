using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VRTrackingApp.Web.Controllers;

public class AccountController : Controller
{
    // Authentication is handled by Windows / Active Directory (Single Sign-On).
    // There is no login form: a domain user is signed in automatically, and the
    // UserAccount table controls who is allowed in and their role.
    // This action is shown when an authenticated domain user is not enrolled.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access denied";
        return View();
    }
}
