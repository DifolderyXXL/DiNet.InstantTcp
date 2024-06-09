namespace DiNet.InstantTcp.Core.Attributes;

[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class ControllerAttribute : Attribute
{
    public string? ControllerKey { get; }
    public ControllerAttribute(string? controllerKey)
    {
        ControllerKey = controllerKey;
    }
}