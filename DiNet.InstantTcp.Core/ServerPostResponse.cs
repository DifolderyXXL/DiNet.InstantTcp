using ASiNet.Data.Serialization;
using DiNet.InstantTcp.Core.Enums;

namespace DiNet.InstantTcp.Core;
public sealed class ServerPostResponse : InstantPackageBase
{
    public ResponseType ResponseType { get; set; }
    public Guid TargetPackageId { get; set; }
    public string? Exception { get; set; }
}

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
