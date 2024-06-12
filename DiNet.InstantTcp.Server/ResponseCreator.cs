using DiNet.InstantTcp.Core;
using System.Linq.Expressions;
namespace DiNet.InstantTcp.Server;

public delegate object BuildResponseDelegate(object value);
public class ResponseCreator
{
    private Dictionary<Type, BuildResponseDelegate> _setValueLambdas = [];

    public object CreateFor(object value)
    {
        var type = value.GetType();

        if (!_setValueLambdas.ContainsKey(type))
        {
            var responseType = typeof(InstantResponse<>).MakeGenericType(type);

            var lambda = BuildLambda(responseType, type);

            _setValueLambdas.Add(type, lambda);
        }

        return _setValueLambdas[type].Invoke(value);
    }

    public object CreateFor(object? value, Type resultType)
    {
        if (!_setValueLambdas.ContainsKey(resultType))
        {
            var responseType = typeof(InstantResponse<>).MakeGenericType(resultType);

            var lambda = BuildLambda(responseType, resultType);

            _setValueLambdas.Add(resultType, lambda);
        }

        return _setValueLambdas[resultType].Invoke(value!);
    }

    public BuildResponseDelegate BuildLambda(Type baseType, Type valueType)
    {
        var p0Param = Expression.Parameter(typeof(object));

        var instVar = Expression.Variable(baseType);
        
        var newInstance = Expression.Assign(instVar, Expression.New(baseType));

        var info = baseType.GetProperty("Value")!;
        var block = Expression.Assign(Expression.Property(instVar, info), Expression.Convert(p0Param, valueType));

        var lambda = Expression.Lambda<BuildResponseDelegate>(
            Expression.Block([instVar], [newInstance, block, instVar]),
            p0Param
            );
        return lambda.Compile();
    }
}
