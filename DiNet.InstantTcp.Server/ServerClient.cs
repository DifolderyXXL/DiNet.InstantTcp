using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Server.Abstraction;
using System.Net.Sockets;

namespace DiNet.InstantTcp.Server;
public class ServerClient : IDisposable
{
    public WrapClient Client { get; }

    private IPackageHandler _packageHandler;

    public ServerClient(TcpClient client, IPackageHandler packageHandler)
    {
        Client = new(client);

        _packageHandler = packageHandler;
        _packageHandler.UpdateClient(Client);
    }

    public void Update()
    {
        var package = Client.Read<InstantPackageBase>();

        if (package is not null)
            _packageHandler.Process(package);
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
