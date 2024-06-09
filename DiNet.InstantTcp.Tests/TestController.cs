using DiNet.InstantTcp.Core.Attributes;

namespace DiNet.InstantTcp.Tests;

[Controller(nameof(TestController))]
public class TestController
{
    [Handler(nameof(Handle))]
    public void Handle()
    {

    }
}
