using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Client.Primitives;
using DiNet.InstantTcp.Core;
using DiNet.IntantTcp.Common.EventStructures;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace DiNet.InstantTcp.Client;
public class TClient
{
    public event Action<ConnectionState>? ConnectionStateChanged;

    public bool Connected => _client is not null && _client.Connected;

    private Dictionary<Type, object> _bridges = [];
    private EventMapper<InstantPackageBase> _bridgeMapper = new();

    private readonly ILogger? _logger;
    private readonly TClientOptions _options;

    private WrapClient? _client;
    private CancellationTokenSource? _acceptorCts;

    private readonly object _lock = new object();

    public TClient(TClientOptions options, ILogger? logger = null)
    {
        _logger = logger;
        _options = options;
    }

    public Bridge<T> GetBridge<T>()
        where T : InstantPackageBase
    {
        Bridge<T>? bridge = null;
        if (_bridges.ContainsKey(typeof(T)))
            bridge = _bridges[typeof(T)] as Bridge<T>;

        if (bridge is null)
        {
            bridge = new Bridge<T>(_options.BridgeCapacity, this, _logger);
            _bridgeMapper.Add<T>(bridge.AddOnBridge);
        }

        return bridge;
    }

    public bool Send<T>(T value)
         where T : InstantPackageBase
    {
        try
        {
            _client!.Send(value);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(TClient.Send));
            return false;
        }
    }

    public async Task<bool> Connect(string ip, int port, CancellationToken token = default)
    {
        try
        {
            if (Connected)
                throw new Exception("Cannot connect when connected.");

            if (_client is not null)
                Disconnect();

            var client = new TcpClient();
            await client.ConnectAsync(ip, port, token);

            var result = Connected;
            if (result)
            {
                _client = new(client);
                ConnectionStateChanged?.Invoke(ConnectionState.Connected);
            }
            
            return result;
        }
        catch(Exception ex)
        {
            _logger?.LogError(ex, nameof(TClient.Connect));
            Disconnect();
            return false;
        }
    }

    public bool AcceptPackages()
    {
        if (_acceptorCts is not null)
            return false;

        _acceptorCts = new();
        _ = AcceptPackagesAsync(_acceptorCts.Token);

        return true;
    }
    public void StopAccept()
    {
        if (_acceptorCts is not null && !_acceptorCts.IsCancellationRequested)
        {
            _acceptorCts.Cancel();
            _acceptorCts.Dispose();
        }

        _acceptorCts = null;
    }

    private async Task AcceptPackagesAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var package = _client!.Read<InstantPackageBase>();

                if (package is not null)
                    _bridgeMapper.InvokeObj(package);
                
                await Task.Delay(_options.PackagePollDelay, token);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(TClient.Connect));
        }
    }

    public bool Disconnect()
    {
        try
        {
            StopAccept();

            _client?.Dispose();
            _client = null;

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, nameof(TClient.Connect));
            return false;
        }
        finally
        {
            ConnectionStateChanged?.Invoke(ConnectionState.Disconnected);
        }
    }
}
