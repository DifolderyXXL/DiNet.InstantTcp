namespace DiNet.InstantTcp.Server;
public class TServerOptions
{
    public string Address { get; set; }
    public int Port { get; set; }

    public TServerOptions(string address, int port)
    {
        Address = address;
        Port = port;
    }

    public int ServerUpdateDelay { get; set; } = 50;
}