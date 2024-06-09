using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;

namespace DiNet.InstantTcp.Server.Abstraction;
public interface IPackageHandler
{
    public void UpdateClient(WrapClient client);
    public void Process(InstantPackageBase package);
}
