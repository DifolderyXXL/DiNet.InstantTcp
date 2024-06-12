using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Tests.Primitives;
using Microsoft.Extensions.Logging;

//"127.0.0.1", 56665
var client = new TClient(new() { BridgeCapacity = 10, PackagePollDelay = 30 }, new CLogger());

while(!await client.Connect("127.0.0.1", 56667))
{
    client.Disconnect();
    Console.WriteLine("TEST");
}

Console.WriteLine("Connected!");

client.AcceptPackages();

var bridge = client.GetInstantBridge<TestPackage2, int>();

bridge.Write(new() { Message = "HELLO WORLD!" });

var msg = await bridge.Read();

Console.WriteLine($"Get msg: {msg?.Value ?? -1}");

Console.ReadLine();

class CLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(state);
    }
}
