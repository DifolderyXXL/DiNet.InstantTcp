using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Server;
using DiNet.InstantTcp.Tests.Primitives;

var server = new TSoloServer(new("127.0.0.1", 56667));

server.SetHandler<TestPackage, int>(x =>
{
    Console.WriteLine(x.Message);
    return 999;
});

server.SetHandler<TestPackage2, int>(x =>
{
    Console.WriteLine(x.Message);
    return 777;
});

server.Start();

await server.Updater(new CancellationTokenSource().Token);