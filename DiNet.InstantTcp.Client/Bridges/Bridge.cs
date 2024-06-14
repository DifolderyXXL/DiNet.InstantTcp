using DiNet.InstantTcp.Core;
using Microsoft.Extensions.Logging;

namespace DiNet.InstantTcp.Client.Bridges;
public class Bridge<TRequest, TResponse> : EventStackBridge<TResponse>
    where TRequest : InstantPackageBase
    where TResponse : InstantPackageBase
{
    private readonly TClient _client;

    public Bridge(int capacity, TClient client, ILogger? logger = null) : base(capacity, logger)
    {
        _client = client;
    }

    public bool Write(TRequest obj)
    {
        try
        {
            _client.Send(obj);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<TRequest, TResponse>.Write));
            return false;
        }
    }
}
