using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace DiNet.InstantTcp.Client;
public class Bridge<T>
{
    private readonly Channel<T> _channel;
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
            
        _channel = Channel.CreateBounded<T>(options);
    }

    internal void AddOnBridge(T obj)
    {
        try
        {
            _channel.Writer.TryWrite(obj);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<T>.AddOnBridge));
        }
    }

    public async Task<T?> Read(CancellationToken token = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(token);
        }
        catch(Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<T>.Read));
            return default;
        }
    }

    public async Task<bool> Write(T obj, CancellationToken token = default)
    {
        try
        {
            await _client.SendAsync(obj, token);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(Bridge<T>.Write));
            return false;
        }
    }
}
