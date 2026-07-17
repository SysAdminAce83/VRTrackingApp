namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Drains the remediation queue and runs each job in its own DI scope so long
/// running installs never block a web request.
/// </summary>
public class RemediationBackgroundService : BackgroundService
{
    private readonly IRemediationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RemediationBackgroundService> _log;

    public RemediationBackgroundService(
        IRemediationQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<RemediationBackgroundService> log)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int jobId;
            try
            {
                jobId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var engine = scope.ServiceProvider.GetRequiredService<RemediationEngine>();
                await engine.RunJobAsync(jobId, stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to run remediation job {JobId}", jobId);
            }
        }
    }
}
