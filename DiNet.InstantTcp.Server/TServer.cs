using DiNet.InstantTcp.Server.Abstraction;
using System.Net;
using System.Net.Sockets;

namespace DiNet.InstantTcp.Server;
public class TSoloServer
{
    private TcpListener _listener;
    private ServerClient? _client;

    private TServerOptions _options;

    private IPackageHandler _packageHandler;

    public TSoloServer(TServerOptions options, IPackageHandler packageHandler)
    {
        _options = options;
        _packageHandler = packageHandler;

        _listener = new(IPAddress.Parse(_options.Address!), _options.Port);
    }

    public void Start()
    {
        _listener.Start();
    }

    public async Task Updater(CancellationToken token)
    {
        await Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                if (_client == null || !_client.Client.Connected)
                {
                    _client = new(_listener.AcceptTcpClient(), _packageHandler);
                }
                else
                {
                    _client.Update();
                    await Task.Delay(_options.ServerUpdateDelay);
                }
            }
        });
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
