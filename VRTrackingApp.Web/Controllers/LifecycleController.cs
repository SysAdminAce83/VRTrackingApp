using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VRTrackingApp.Web.Services.Exceptions;

namespace VRTrackingApp.Web.Controllers;

[Authorize(Roles = "Admin")]
public class LifecycleController : Controller
{
    private readonly ExceptionLifecycleService _lifecycle;
    public LifecycleController(ExceptionLifecycleService lifecycle) => _lifecycle = lifecycle;

    /// <summary>Manually trigger the exception lifecycle sweep (expiry / review / reminders).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run()
    {
        await _lifecycle.RunAsync();
        return RedirectToAction("Index", "Exceptions");
    }
}
