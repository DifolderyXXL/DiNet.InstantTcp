using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Core;
using System.Net.Sockets;

namespace DiNet.InstantTcp.Client;

/// <summary>
/// Wrap on TcpClient, sends and reads only InstantPackageBase types.
/// </summary>
public class WrapClient : IDisposable
{
    public bool Connected => IsConnectedCheck();

    public TcpClient TcpClient { get; }
    public NetworkStream NetworkStream { get; }

    public WrapClient(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        NetworkStream = TcpClient.GetStream();
    }

    public void Send<T>(T value)
        where T : InstantPackageBase
    {
        if (!Connected)
            throw new Exception("Client is not connected.");

        BinarySerializer.Serialize<InstantPackageBase>(value, NetworkStream!);
    }
    public T? Read<T>() 
        where T : InstantPackageBase
    {
        if (!Connected)
            throw new Exception("Client is not connected.");

        if (NetworkStream!.DataAvailable)
            return BinarySerializer.Deserialize<InstantPackageBase>(NetworkStream) as T;
        
        return null;
    }

    private bool IsConnectedCheck()
    {
        try
        {
            if (TcpClient != null && TcpClient.Client != null && TcpClient.Client.Connected)
            {
                if (TcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (TcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        return false;
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public void Dispose()
    {
        TcpClient?.Dispose();

        if (NetworkStream is not null) NetworkStream.Dispose();
    }
}
