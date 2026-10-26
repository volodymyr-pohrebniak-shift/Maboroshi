using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Globalization;

namespace Maboroshi.TemplateEngine;

public abstract record ReturnType
{
    public abstract object GetValue();

    // how many arrays are nested in the value; printing and json are recursive, so it's kept within MaxDepth
    internal virtual int Depth => 0;

    // the printed length of the value; arrays add it up from their items without printing them,
    // so a value can be checked against the limits before anything huge is materialized
    internal virtual long Size => ToString().Length;
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
    internal const long MaxSafeInteger = 9_007_199_254_740_991;

    public double Value { get; }

    public NumberReturn(double value) => Value = double.IsFinite(value)
        ? value
        : throw new TemplateRuntimeException($"The result is not a finite number ({value.ToString(CultureInfo.InvariantCulture)}), probably a division by zero");

    public override object GetValue() => Value;
    public static implicit operator NumberReturn(double value) => new(value);

    public static implicit operator double(NumberReturn result) => result.Value;

    // whole numbers are printed without an exponent (1E+15) while they are exact
    public override string ToString() => Value == Math.Floor(Value) && Math.Abs(Value) <= MaxSafeInteger
        ? Value.ToString("F0", CultureInfo.InvariantCulture)
        : Value.ToString(CultureInfo.InvariantCulture);
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
    public ArrayReturn(T[] values)
    {
        Values = values;
        Depth = 1 + (values.Length == 0 ? 0 : values.Max(v => v.Depth));
        Size = values.Sum(v => v.Size) + Math.Max(0, values.Length - 1);
    }
    internal override int Depth { get; }
    internal override long Size { get; }
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