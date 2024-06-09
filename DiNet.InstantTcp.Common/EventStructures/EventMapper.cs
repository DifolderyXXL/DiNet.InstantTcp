using DiNet.IntantTcp.Common.Generation;

namespace DiNet.IntantTcp.Common.EventStructures;
public class EventMapper<TBaseKey>
{
    private Dictionary<Type, Action<object, object>> _callLambdas = [];
    private Dictionary<Type, object> _delegates = [];

    public void Add<TKey>(Action<TKey> lambda)
        where TKey : class, TBaseKey
    {
        _delegates.TryAdd(typeof(TKey), new MappedBox<TKey>());
        (_delegates[typeof(TKey)] as MappedBox<TKey>)!.Subscribe(lambda);
    }

    public void Remove<TKey>(Action<TKey> lambda)
    where TKey : class, TBaseKey
    {
        if (_delegates.ContainsKey(typeof(TKey)))
            (_delegates[typeof(TKey)] as MappedBox<TKey>)!.Unsubscribe(lambda);
    }

    public void Invoke<TKey>(TKey value)
        where TKey : class, TBaseKey
    {
        if (_delegates.ContainsKey(typeof(TKey)))
            (_delegates[typeof(TKey)] as MappedBox<TKey>)!.Invoke(value);
    }

    public void InvokeObj(TBaseKey value)
    {
        if (value is null) return;

        var type = value.GetType();
        if (_delegates.ContainsKey(type))
            GetOrCreateLambda(type).Invoke(_delegates[type], value);
    }

    private Action<object, object> GetOrCreateLambda(Type type)
    {
        if (!_callLambdas.ContainsKey(type))
            _callLambdas.Add(type, new MethodQuery(_delegates[type].GetType()).Select("Invoke").BuildLambda(type));

        return _callLambdas[type];
    }
}

public class MappedBox<T>
{
    private event Action<T>? _action;

    public void Invoke(T value)
    {
        _action?.Invoke(value);
    }

    public void Subscribe(Delegate lambda)
    {
        if (lambda is Action<T> action)
            _action += action;
    }

    public void Unsubscribe(Delegate lambda)
    {
        if (lambda is Action<T> action)
            _action -= action;
    }
}