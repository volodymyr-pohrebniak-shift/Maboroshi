using Bogus;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Maboroshi.TemplateEngine;

interface IFakerAdapter
{
    public ReturnType? GetFakeValue(string name, params ReturnType[] args);
}

internal class FakerAdapter : IFakerAdapter
{
    private static readonly Faker _faker = new();

    public ReturnType? GetFakeValue(string name, params ReturnType[] additionalArguments)
    {
        return new StringReturn("");
    }
}

internal class FakerDynamicResolver : IFakerAdapter
{
    private static readonly Faker _faker = new();
    private static readonly ConcurrentDictionary<string, Func<object, object>> _propertiesCache = [];

    public ReturnType? GetFakeValue(string path, params ReturnType[] additionalArguments)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path to faker property cannot be  null or empty");
        }

        if (!_propertiesCache.TryGetValue(path, out var compiledDelegate))
        {
            compiledDelegate = CompileDelegate(path);
            _propertiesCache[path] = compiledDelegate;
        }

        return new StringReturn((string) compiledDelegate(_faker));
    }

    private static Func<object, object> CompileDelegate(string path) {
        var pathParts = path.Split('.');

        var parameter = Expression.Parameter(typeof(object), "faker");
        Expression currentExpression = Expression.Convert(parameter, typeof(Faker));

        foreach (var pathPart in pathParts)
        {
            var currentType = currentExpression.Type;

            var property = currentType.GetProperty(pathPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property != null)
            {
                currentExpression = Expression.Property(currentExpression, property);
            } else
            {
                var method = currentType.GetMethod(pathPart, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    var argumentExpressions = new List<Expression>();

                    foreach (var param in parameters)
                    {
                        if (param.HasDefaultValue)
                        {
                            argumentExpressions.Add(Expression.Constant(param.DefaultValue, param.ParameterType));
                        }
                        else
                        {
                            throw new InvalidOperationException($"Faker property ${path} requires parameters. It's not supported yet");
                        }
                    }

                    currentExpression = Expression.Call(currentExpression, method, argumentExpressions);
                }
                else
                {
                    throw new InvalidOperationException($"'{pathPart}' is not a valid property or method of '{currentType.Name}'.");
                }
            }
        }

        currentExpression = Expression.Convert(currentExpression, typeof(object));

        return Expression.Lambda<Func<object, object>>(currentExpression, parameter).Compile();
    }
}
