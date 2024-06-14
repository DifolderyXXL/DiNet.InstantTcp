namespace DiNet.InstantTcp.Client.Bridges;
public abstract class BridgeBase<TPackage>
{
    internal abstract void AddOnBridge(TPackage obj);
}
