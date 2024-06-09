using System.Linq.Expressions;
using System.Reflection;

namespace DiNet.IntantTcp.Common.Generation;
public class MethodQuery
{
    public MethodQuery(Type type)
    {
        _baseType = type;
    }

    public MethodQuery(MethodInfo methodInfo)
    {
        _baseType = methodInfo.DeclaringType ?? throw new Exception();
        _method = methodInfo;
        _parameters = _method.GetParameters();
        _set = new Expression[_parameters.Length];
    }

    private Type _baseType;

    private MethodInfo? _method;

    private ParameterInfo[]? _parameters;

    private Expression[]? _set;

    public Type BaseType => _baseType;


    public bool IsGeneric => _method?.IsGenericMethod ?? throw new Exception();

    public ParameterInfo[] Parameters => _parameters ?? throw new Exception();

    public MethodQuery Select(string methodName)
    {
        var method = _baseType.GetMethod(methodName);
        _method = method;
        _parameters = _method!.GetParameters();
        _set = new Expression[_parameters.Length];
        return this;
    }

    public Action<object, object> BuildLambda(Type p0Type)
    {
        var instParam = Expression.Parameter(typeof(object));
        var p0Param = Expression.Parameter(typeof(object));
        var lambda = Expression.Lambda<Action<object, object>>(Expression.Call(Expression.Convert(instParam, BaseType), _method!, Expression.Convert(p0Param, p0Type)), instParam, p0Param);
        return lambda.Compile();
    }

    public Action<object, object, object> BuildLambda(Type p0Type, Type p1Type)
    {
        var instParam = Expression.Parameter(typeof(object));
        var p0Param = Expression.Parameter(typeof(object));

        var p1Param = Expression.Parameter(typeof(object));
        var lambda = Expression.Lambda<Action<object, object, object>>(
            Expression.Call(
                Expression.Convert(instParam, BaseType), _method!, 
                Expression.Convert(p0Param, p0Type), Expression.Convert(p1Param, p1Type)), instParam, p0Param, p1Param);
        return lambda.Compile();
    }

    public MethodQuery SetParameter(int index, object value, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        _set[index] = convertToParameterType ?
            Expression.Convert(Expression.Constant(value), _parameters[index].ParameterType) :
            Expression.Constant(value);
        return this;
    }

    public MethodQuery SetParameter(string name, object value, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        var parameter = _parameters.FirstOrDefault(x => x.Name == name) ?? throw new Exception();
        _set[parameter.Position] = convertToParameterType ?
            Expression.Convert(Expression.Constant(value), _parameters[parameter.Position].ParameterType) :
            Expression.Constant(value);
        return this;
    }

    public MethodQuery SetParameter(int index, object? value, Type type, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        _set[index] = convertToParameterType ?
            Expression.Convert(Expression.Constant(value, type), _parameters[index].ParameterType) :
            Expression.Constant(value, type);
        return this;
    }

    public MethodQuery SetParameter(string name, object? value, Type type, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        var parameter = _parameters.FirstOrDefault(x => x.Name == name) ?? throw new Exception();
        _set[parameter.Position] = convertToParameterType ?
            Expression.Convert(Expression.Constant(value, type), _parameters[parameter.Position].ParameterType) :
            Expression.Constant(value, type);
        return this;
    }

    public MethodQuery SetParameter(int index, Expression value, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        _set[index] = convertToParameterType ?
            Expression.Convert(value, _parameters[index].ParameterType) :
            value;
        return this;
    }

    public MethodQuery SetParameter(string name, Expression value, bool convertToParameterType = false)
    {
        if (_parameters is null || _set is null)
            throw new Exception();
        var parameter = _parameters.FirstOrDefault(x => x.Name == name) ?? throw new Exception();
        _set[parameter.Position] = convertToParameterType ?
            Expression.Convert(value, _parameters[parameter.Position].ParameterType) :
            value;
        return this;
    }

    public Expression Build(object instance)
    {
        if (_set is null || _method is null)
            throw new Exception();
        var inst = Expression.Constant(instance);
        return Expression.Call(inst, _method, _set);
    }

    public Expression Build(Expression instance)
    {
        if (_set is null || _method is null)
            throw new Exception();
        return Expression.Call(instance, _method, _set);
    }

    public Action BuildLambda(object instance)
    {
        var body = Build(instance);
        var lambda = Expression.Lambda<Action>(body);
        return lambda.Compile();
    }
}
