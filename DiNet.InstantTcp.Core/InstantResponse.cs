using DiNet.InstantTcp.Core.Enums;

namespace DiNet.InstantTcp.Core;
public class InstantResponse<TValue> : InstantResponseBase
{
    public TValue? Value { get; set; }
}

public abstract class InstantResponseBase: InstantPackageBase
{
    public ResponseType ResponseType { get; set; }
    public Guid TargetPackageId { get; set; }
    public string? Exception { get; set; }
}
