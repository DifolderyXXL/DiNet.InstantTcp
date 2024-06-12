using DiNet.InstantTcp.Core;
using DiNet.InstantTcp.Server;


var responseBuilder = new ResponseCreator();
var obj = (responseBuilder.CreateFor(25) as InstantResponse<int>);

Console.WriteLine(obj.Value);

return 0;
var handler = new PackageHandler();

handler.Add<P1, int>(x => 15);
handler.Add<P2, int>(x => 33);

Console.WriteLine(handler.InvokeObj(new P1()));
Console.WriteLine(handler.InvokeObj(new P2()));

class P1 : InstantPackageBase
{

}
class P2 : InstantPackageBase
{

}