using System.Threading.Channels;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>In-process queue of remediation job ids awaiting background execution.</summary>
public interface IRemediationQueue
{
    ValueTask EnqueueAsync(int jobId, CancellationToken ct = default);
    ValueTask<int> DequeueAsync(CancellationToken ct);
}

public class RemediationQueue : IRemediationQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

    public ValueTask EnqueueAsync(int jobId, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(jobId, ct);

    public ValueTask<int> DequeueAsync(CancellationToken ct) =>
        _channel.Reader.ReadAsync(ct);
}
