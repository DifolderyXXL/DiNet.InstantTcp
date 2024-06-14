using DiNet.InstantTcp.Core;
using Microsoft.Extensions.Logging;

namespace DiNet.InstantTcp.Client.Bridges;
public class EventBridge<TEvent> : BridgeBase<TEvent>
    where TEvent : InstantPackageBase
{
    public event Action<TEvent>? OnEvent;

    private readonly ILogger? _logger;

    public EventBridge(ILogger? logger = null)
    {
        _logger = logger;
    }

    internal override void AddOnBridge(TEvent obj)
    {
        try
        {
            OnEvent?.Invoke(obj);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(EventStackBridge<TEvent>.AddOnBridge));
        }
    }
}