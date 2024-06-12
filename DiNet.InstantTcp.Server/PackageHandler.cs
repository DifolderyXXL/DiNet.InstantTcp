using DiNet.InstantTcp.Common.Generation;

namespace DiNet.InstantTcp.Server;
public class PackageHandler
{
    private Dictionary<Type, Func<object, object, object>> _callLambdas = [];
    private Dictionary<Type, object> _delegates = [];

    public void Add<TRequest, TResponse>(Func<TRequest, TResponse> lambda)
    {
        _delegates.TryAdd(typeof(TRequest), new BoxedFunc<TRequest, TResponse>(lambda));
    }

    public object InvokeObj(object value)
    {
        var type = value.GetType();
        return GetOrCreateLambda(type).Invoke(_delegates[type], value);
    }

    private Func<object, object, object> GetOrCreateLambda(Type type)
    {
        if (!_callLambdas.ContainsKey(type))
            _callLambdas.Add(type, new MethodQuery(_delegates[type].GetType()).Select("Invoke").BuildLambdaFunc(type));

        return _callLambdas[type];
    }
}

public class BoxedFunc<TRequest, TResponse>
{
    private Func<TRequest, TResponse> _func;

    public BoxedFunc(Func<TRequest, TResponse> func)
    {
        _func = func;
    }

    public TResponse Invoke(TRequest request)
    {
        return _func.Invoke(request);
    }
}
