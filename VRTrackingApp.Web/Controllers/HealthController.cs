using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly VRTrackingAppContext _db;
    private readonly ILogger<HealthController> _logger;

    public HealthController(VRTrackingAppContext db, ILogger<HealthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var checks = new Dictionary<string, object>();

        try
        {
            var canConnect = await _db.Database.CanConnectAsync(ct);
            checks["database"] = new { status = canConnect ? "Healthy" : "Unhealthy" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            checks["database"] = new { status = "Unhealthy", error = ex.Message };
        }

        var healthy = checks.Values.Cast<dynamic>().All(c => ((string)c.status) == "Healthy");
        var result = new { status = healthy ? HealthStatus.Healthy.ToString() : HealthStatus.Unhealthy.ToString(), checks };
        return Ok(result);
    }
}
