using DiNet.InstantTcp.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DiNet.InstantTcp.Client.Bridges;
public class EventStackBridge<TEvent>
    where TEvent : InstantPackageBase
{
    public event Action? OnEventAdded;

    private readonly Channel<TEvent> _channel;
    private readonly ILogger? _logger;

    public EventStackBridge(int capacity, ILogger? logger = null)
    {
        _logger = logger;

        var options = new BoundedChannelOptions(capacity)
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        };

        _channel = Channel.CreateBounded<TEvent>(options);
    }

    internal void AddOnBridge(TEvent obj)
    {
        try
        {
            _channel.Writer.TryWrite(obj);
            OnEventAdded?.Invoke();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(EventStackBridge<TEvent>.AddOnBridge));
        }
    }

    public async Task<TEvent?> Read(CancellationToken token = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(token);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(EventStackBridge<TEvent>.Read));
            return default;
        }
    }
}
