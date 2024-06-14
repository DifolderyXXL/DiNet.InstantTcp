using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Client.Bridges;
using DiNet.InstantTcp.Client.Primitives;
using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Common.EventStructures;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System;

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

    private TBridgeType GetOrCreateBridge<TKey, TBridgeType>(Func<TBridgeType> factory)
        where TBridgeType : BridgeBase<TKey>
        where TKey : InstantPackageBase
    {
        if (_bridges.ContainsKey(typeof(TKey)))
            return (TBridgeType)_bridges[typeof(TKey)];

        var newBridge = factory.Invoke();
        _bridgeMapper.Add<TKey>(newBridge.AddOnBridge);
        return newBridge;
    }

    public Bridge<TRequest, TResponse> GetBridge<TRequest, TResponse>()
        where TRequest : InstantPackageBase
        where TResponse : InstantPackageBase
        => GetOrCreateBridge<TResponse, Bridge<TRequest, TResponse>>(
            () => new Bridge<TRequest, TResponse>(_options.BridgeCapacity, this, _logger));
    
    public Bridge<TRequest, InstantResponse<TResponse>> GetInstantBridge<TRequest, TResponse>()
        where TRequest : InstantPackageBase
        => GetBridge<TRequest, InstantResponse<TResponse>>();

    public EventStackBridge<TEvent> GetEventStackBridge<TEvent>()
        where TEvent : InstantPackageBase
        => GetOrCreateBridge<TEvent, EventStackBridge<TEvent>>(
            () => new EventStackBridge<TEvent>(_options.BridgeCapacity, _logger));
    
    public EventBridge<TEvent> GetEventBridge<TEvent>()
        where TEvent : InstantPackageBase
        => GetOrCreateBridge<TEvent, EventBridge<TEvent>>(
            () => new EventBridge<TEvent>(_logger));
    
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
            _logger?.LogError(ex, $"{nameof(TClient.Send)}, {ex.ToString()}");
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

            _client = new(client);

            var result = Connected;
            if (result)
            {
                ConnectionStateChanged?.Invoke(ConnectionState.Connected);
            }
            
            return result;
        }
        catch(Exception ex)
        {
            _logger?.LogError(ex, $"{nameof(TClient.Connect)}, {ex.ToString()}");
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
                {
                    _bridgeMapper.InvokeObj(package);
                }
                
                await Task.Delay(_options.PackagePollDelay, token);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{nameof(TClient.AcceptPackagesAsync)}, {ex.ToString()}");
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