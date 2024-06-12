namespace DiNet.InstantTcp.Controllers.GenerationModel;
public class ControllerBuilder
{
    private readonly Type _currentType;
    private readonly object _controllerInstance;

    public ControllerBuilder(object controllerInstance)
    {
        _currentType = controllerInstance.GetType();
        _controllerInstance = controllerInstance;
    }

    public ControllerModel Build()
    {
        throw new NotImplementedException();
    }
}
