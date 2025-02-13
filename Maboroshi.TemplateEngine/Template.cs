using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Text;

namespace Maboroshi.TemplateEngine;

public abstract record ReturnType
{
    public abstract object GetValue();
}

public sealed record StringReturn : ReturnType
{
    public string Value { get; }
    public StringReturn(string value) => Value = value;
    public override object GetValue() => Value;

    public static implicit operator StringReturn(string value) => new(value);

    public static implicit operator string(StringReturn result) => result.Value;
}

public sealed record ArrayReturn<T> : ReturnType where T : ReturnType
{
    public T[] Values { get; }
    public ArrayReturn(T[] values) => Values = values;
    public override object GetValue() => Values;
}

public class Template
{
    private readonly List<TemplateNode> _nodes;
    private readonly IFunctionResolver[] _functionResolvers;

    internal Template(List<TemplateNode> nodes, string content, IFunctionResolver[] functionResolvers)
    {
        _nodes = nodes;
        Content = content;
        _functionResolvers = functionResolvers;
    }

    public string Content { get; }

    public string Compile(params IFunctionResolver[] additionalFunctionResolvers)
    {
        var resolvers = _functionResolvers.Concat(additionalFunctionResolvers);

        var templateNodeVisitor = new TemplateNodeVisitor(new TemplateContext(), resolvers);

        var sb = new StringBuilder();
        foreach(var node in _nodes)
        {
            var result = node.Accept(templateNodeVisitor);
            if (result is StringReturn str)
                sb.Append(str);
        }

        return sb.ToString();
    }
}