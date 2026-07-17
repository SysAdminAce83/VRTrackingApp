using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VRTrackingApp.Web.Services.Exceptions;

/// <summary>
/// Hosted timer that runs <see cref="ExceptionLifecycleService"/> on a fixed interval
/// (default 5 minutes) so exceptions expire / come up for review and send reminders
/// without user interaction. Each run executes in its own DI scope.
/// </summary>
public class ExceptionLifecycleHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExceptionLifecycleHostedService> _log;
    private readonly TimeSpan _interval;

    public ExceptionLifecycleHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<ExceptionReminderOptions> options,
        ILogger<ExceptionLifecycleHostedService> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
        _interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds > 0 ? options.Value.IntervalSeconds : 300);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small delay so startup isn't blocked; first sweep runs shortly after launch.
        try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<ExceptionLifecycleService>();
                await svc.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _log.LogError(ex, "Exception lifecycle sweep failed.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
