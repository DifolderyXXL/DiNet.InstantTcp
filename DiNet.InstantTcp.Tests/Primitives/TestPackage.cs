using ASiNet.Data.Serialization.Attributes;
using DiNet.InstantTcp.Core;

namespace DiNet.InstantTcp.Tests.Primitives;

[PreGenerate]
public class TestPackage : InstantPackageBase
{
    public string? Message { get; set; }
}

[PreGenerate]
public class TestPackage2 : InstantPackageBase
{
    public string? Message { get; set; }
}
