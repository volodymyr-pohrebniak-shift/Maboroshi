using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Globalization;

namespace Maboroshi.TemplateEngine;

public abstract record ReturnType
{
    public abstract object GetValue();
}

public sealed record BoolReturn : ReturnType
{
    public bool Value { get; }
    public BoolReturn(bool value) => Value = value;
    public override object GetValue() => Value;
    public static implicit operator BoolReturn(bool value) => new(value);

    public static implicit operator bool(BoolReturn result) => result.Value;
    public override string ToString() => Value.ToString().ToLowerInvariant();
}

public sealed record NumberReturn : ReturnType
{
    public double Value { get; }
    public NumberReturn(double value) => Value = value;
    public override object GetValue() => Value;
    public static implicit operator NumberReturn(double value) => new(value);

    public static implicit operator double(NumberReturn result) => result.Value;
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

public sealed record StringReturn : ReturnType
{
    public string Value { get; }
    public StringReturn(string value) => Value = value;
    public override object GetValue() => Value;

    public static implicit operator StringReturn(string value) => new(value);

    public static implicit operator string(StringReturn result) => result.Value;
    public override string ToString() => Value;
}

public sealed record ArrayReturn<T> : ReturnType where T : ReturnType
{
    public T[] Values { get; }
    public ArrayReturn(T[] values) => Values = values;
    public override object GetValue() => Values;
    public override string ToString() => string.Join(",", Values.Select(v => v.ToString()));
}

public class Template
{
    private readonly List<TemplateNode> _nodes;
    private readonly IFunctionResolver[] _functionResolvers;
    private readonly TemplateCompilationOptions _options;

    internal Template()
    {
        _nodes = [];
        _functionResolvers = [];
        _options = new TemplateCompilationOptions(false);
    }

    internal Template(List<TemplateNode> nodes, IFunctionResolver[] functionResolvers, TemplateCompilationOptions options)
    {
        _nodes = nodes;
        _functionResolvers = functionResolvers;
        _options = options;
    }

    public virtual string Compile(params IFunctionResolver[] additionalFunctionResolvers)
    {
        var templateContext = new TemplateContext();
        var resolvers = _functionResolvers
            .Concat(additionalFunctionResolvers)
            .Append(new VariablesFunctionResolver(templateContext))
            .ToList();

        var templateNodeVisitor = new TemplateNodeVisitor(templateContext, resolvers, _options);

        return templateNodeVisitor.RenderAll(_nodes);
    }
}