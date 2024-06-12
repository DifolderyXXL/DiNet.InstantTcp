using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;
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

    public TSoloServer(TServerOptions options)
    {
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

    public async Task Updater(CancellationToken token)
    {
        await Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
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
        }

        response.TargetPackageId = package.Id;

        try
        {
            _client!.Send<InstantPackageBase>(response!);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Console.WriteLine(ex);
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