namespace DiNet.InstantTcp.Core.Attributes;

[System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class HandlerAttribute : Attribute
{
    public string? HandlerKey { get; }
    public HandlerAttribute(string? handlerKey)
    {
        HandlerKey = handlerKey;
    }
}
