using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace DiNet.InstantTcp.Server;

public delegate TResult PackageHandlerDelegate<T, TResult>(T package)
    where T: InstantPackageBase;
public class TSoloServer
{
    private TcpListener _listener;
    private WrapClient? _client;

    private TServerOptions _options;

    private PackageHandler _handlerMapper = new();
    private ResponseCreator _responseCreator = new();

    private Dictionary<Type, Type> _resultTypes = [];

    private readonly ILogger? _logger;

    public TSoloServer(TServerOptions options, ILogger? logger = null)
    {
        _logger = logger;
        _options = options;

        _listener = new(IPAddress.Parse(_options.Address!), _options.Port);
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    /// <typeparam name="TResult"> generic parameter of InstantResponse<TResult> </typeparam>
    /// <param name="action"></param>
    public void SetHandler<TPackage, TResult>(Func<TPackage, TResult> action)
        where TPackage : InstantPackageBase
    {
        _handlerMapper.Add(action);
        _resultTypes.Add(typeof(TPackage), typeof(TResult));
    }

    public void Start()
    {
        _listener.Start();
    }

    /// <summary>
    /// Handles requests from client ans sends InstantResponse<> them
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task Updater(CancellationToken token)
    {
        await Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_client == null || !_client.Connected)
                    {
                        _client = new(_listener.AcceptTcpClient());
                    }
                    else
                    {
                        var package = _client.Read<InstantPackageBase>();
                        if (package is not null)
                            ProcessRecievedPackage(package);
                    }
                }
                catch(Exception ex)
                {
                    _logger?.LogError(ex, $"{nameof(Updater)}, {ex.ToString()}");
                }
                Task.Delay(_options.ServerUpdateDelay);
            }
        });
    }

    private void ProcessRecievedPackage(InstantPackageBase package)
    {
        var packageType = package.GetType();

        InstantResponseBase? response;
        try
        {
            var result = _handlerMapper.InvokeObj(package);

            response = (_responseCreator.CreateFor(result) as InstantResponseBase)!;
            response.ResponseType = Core.Enums.ResponseType.Ok;
        }
        catch (Exception ex)
        {
            response = (_responseCreator.CreateFor(null, _resultTypes[packageType]) as InstantResponseBase)!;
            response.ResponseType = Core.Enums.ResponseType.Exception;
            response.Exception = ex.ToString();

            _logger?.LogWarning(ex, $"{nameof(ProcessRecievedPackage)}, {ex.ToString()}");
        }

        response.TargetPackageId = package.Id;

        try
        {
            _client!.Send<InstantPackageBase>(response!);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{nameof(ProcessRecievedPackage)}, {ex.ToString()}");
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
        _listener?.Dispose();
        GC.SuppressFinalize(this);
    }
    ~TSoloServer()
    {
        Dispose();
    }
}