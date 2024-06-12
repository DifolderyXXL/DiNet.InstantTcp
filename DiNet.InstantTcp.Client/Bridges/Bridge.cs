using DiNet.InstantTcp.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DiNet.InstantTcp.Client.Bridges;
public class Bridge<TRequest, TResponse>
    where TRequest : InstantPackageBase
    where TResponse : InstantPackageBase
{
    private readonly Channel<TResponse> _channel;
    private readonly ILogger? _logger;
    private readonly TClient _client;

    public Bridge(int capacity, TClient client, ILogger? logger = null)
    {
        _logger = logger;
        _client = client;

        var options = new BoundedChannelOptions(capacity)
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        };

        _channel = Channel.CreateBounded<TResponse>(options);
    }

    internal void AddOnBridge(TResponse obj)
    {
        try
        {
            _channel.Writer.TryWrite(obj);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<TRequest, TResponse>.AddOnBridge));
        }
    }

    public async Task<TResponse?> Read(CancellationToken token = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(token);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<TRequest, TResponse>.Read));
            return default;
        }
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
