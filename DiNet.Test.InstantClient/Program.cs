using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Client;
using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Tests.Primitives;


BinarySerializer.SerializeContext.GetOrGenerate<InstantResponse<int>>();

//"127.0.0.1", 56665
var client = new TClient(new() { BridgeCapacity = 10, PackagePollDelay = 30 });

while(!await client.Connect("127.0.0.1", 56667))
    client.Disconnect();

client.AcceptPackages();

var bridge = client.GetInstantBridge<TestPackage, int>();

bridge.Write(new() { Message = "HELLO WORLD!" });

var msg = await bridge.Read();

Console.WriteLine(msg);

Console.ReadLine();

