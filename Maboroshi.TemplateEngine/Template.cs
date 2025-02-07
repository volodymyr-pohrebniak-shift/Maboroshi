using Maboroshi.TemplateEngine.TemplateNodes;
using System.Text;

namespace Maboroshi.TemplateEngine;

public interface IExpressionValueResolver
{
    string? TryResolve(string name, params object[] additionalArguments);
}

public class StringExpressionResolver : IExpressionValueResolver
{
    public string? TryResolve(string name, params object[] additionalArguments)
    {
        return null;
    }
}


internal record TemplateError;

internal record Result<T>(T value)
{

}


class TemplateNodeVisitor
{
    public Result<string> Visit(TemplateNode node)
    {
        return null;
    }
}

public class Template
{
    internal Template(string content)
    {
        Content = content;
    }

    private readonly IExpressionValueResolver _fakerResolver = new FakerDynamicResolver();
    private IExpressionValueResolver? _httpRequestResolver;

    public string Content { get; }

    public string Compile(IExpressionValueResolver httpRequestResolver)
    {
        _httpRequestResolver = httpRequestResolver;
        var lexer = new Lexer(Content);
        var parser = new Parser(lexer.Tokenize().ToList());
        var nodes = parser.Parse();

        var context = new TemplateContext();

        var sb = new StringBuilder();
        foreach(var node in nodes)
        {
            sb.Append(Visit(node, context));
        }

        return sb.ToString();
    }

    private string Visit(TemplateNode node,TemplateContext context)
    {
        return node switch
        {
            TextNode textNode => textNode.Value,
            ExpressionNode expressionNode => VisitExpression(expressionNode, context).ToString()!,
            BlockNode blockNode => VisitBlock(blockNode, context),
            _ => throw new InvalidOperationException($"No visitor for node type {node.GetType()}"),
        };
    }

    private object VisitExpression(ExpressionNode node, TemplateContext context)
    {
        var firstNode = node.Parameters[0];

        if (firstNode is LiteralNode literalNode)
        {
            return literalNode.Value;
        }
        else if (firstNode is VariableNode variableNode)
        {
            return context.GetVariable(variableNode.Value);
        }
        else if (firstNode is FunctionNode identifierNode)
        {
            return EvaluateIdentifier(identifierNode, node.Parameters.Skip(1).ToList(), context);
        } else
        {
            throw new Exception("Non-block expressions have to start with either identifier or variable");
        }
    }

    private object EvaluateIdentifier(FunctionNode node, List<TemplateNode> parameters, TemplateContext context)
    {
        switch(node.Name.ToLower())
        {
            case "var":
                if (parameters.Count < 2)
                    throw new Exception($"var identifier should have two parameters");
                
                var evaluatedParans = parameters.Select(x => EvaluateParameter(x, context)).ToList();
                if (evaluatedParans[0] is String strParam)
                    context.SetVariable(strParam, evaluatedParans[1]);
                else
                    throw new Exception($"var first parameter should be string");

                return string.Empty;
            
            // arrays
            case "array":
                if (parameters.Count == 0)
                    throw new Exception($"at least one item should be provided for an array");
                var a = parameters.Select(x => EvaluateParameter(x, context).ToString()).ToArray();
                return a;
            case "concat":
                break;
            case "oneof":
                if (parameters.Count == 0)
                    throw new Exception($"oneof requires a parameter");
                var parameter = EvaluateParameter(parameters[0], context);
                if (parameter is Array array)
                {
                    var index = new Random().Next(0, array.Length);
                    return array.GetValue(index)!;
                }
                throw new Exception($"oneof requires an array parameter");

            // strings

            case "substring":
                break;
            case "lowercase":
                if (parameters.Count == 0)
                    throw new Exception($"lowercase requires a parameter");
                var lowercaseParameter = EvaluateParameter(parameters[0], context);
                if (lowercaseParameter is string str)
                {
                    return str.ToLower();
                }
                throw new Exception($"lowercase requires a string parameter");
            case "uppercase":
                if (parameters.Count == 0)
                    throw new Exception($"lowercase requires a parameter");
                var uppercaseParameter = EvaluateParameter(parameters[0], context);
                if (uppercaseParameter is string str1)
                {
                    return str1.ToUpper();
                }
                throw new Exception($"lowercase requires a string parameter");

            // faker

            case "faker":
                if (parameters.Count == 0)
                    throw new Exception($"lowercase requires a parameter");
                var fakerParameter = EvaluateParameter(parameters[0], context);

                if (fakerParameter is string str2)
                {
                    return _fakerResolver.TryResolve(str2)!;
                }
                throw new Exception($"lowercase requires a string parameter");

            // http request info


            default:
                throw new Exception($"Unknown identifier {node.Name}");
        }

        return string.Empty;
    }

    private object EvaluateParameter(TemplateNode node, TemplateContext context)
    {
        return node switch
        {
            ExpressionNode subExpressionNode => VisitExpression(subExpressionNode, context),
            LiteralNode literalNode => literalNode.Value,
            VariableNode variableNode => context.GetVariable(variableNode.Value),
            _ => throw new Exception("Wrong type for the parameter"),
        };
    }

    private string VisitBlock(BlockNode node, TemplateContext context) {
        switch(node.Name)
        {
            case "repeat":
                if (node.Parameters.Count == 0)
                    throw new Exception($"repeat block should have at least one parameter");
                var evaluatedParameters = node.Parameters.Select(x => EvaluateParameter(x, context)).ToList();
                var repeatCount = -1;
                if (int.TryParse(evaluatedParameters[0].ToString(), out var minCount))
                {
                    if (evaluatedParameters.Count > 1)
                    {
                        if (int.TryParse(evaluatedParameters[0].ToString(), out var maxCount))
                        {
                            repeatCount = new Random().Next(minCount, maxCount + 1);
                        } else
                        {
                            throw new Exception($"repeat block parameters should be int");
                        }
                    } else
                    {
                        repeatCount = minCount;
                    }
                    var sb = new StringBuilder();
                    var nestedContext = new TemplateContext(context);
                    for(var i = 0; i < repeatCount; i++)
                    {
                        nestedContext.SetVariable("index", i);
                        foreach(var body in node.Body)
                        {
                            sb.Append(Visit(body, nestedContext));
                        }
                    }
                    return sb.ToString();
                } else
                {
                    throw new Exception($"repeat block parameters should be int");
                }
            default:
                return string.Empty;
        }
    }
}

public class StaticExpressionValueResolver : IExpressionValueResolver
{
    public string? TryResolve(string name, params object[] additionalArguments)
    {
        return name.ToLower() switch
        {
            "var" => EvaluateVariableSetting(),
            _ => null
        };
    }

    private string EvaluateVariableSetting()
    {
        return "";
    }
}

public class TemplateContext
{
    private readonly TemplateContext? _parent;
    private readonly Dictionary<string, object> _variables = [];

    public TemplateContext() { }

    public TemplateContext(TemplateContext? parent)
    {
        _parent = parent;
    }

    public object GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out object? value))
            return value;
        else if (_parent is not null)
            return _parent.GetVariable(name);
        throw new Exception($"Variable {name} doesn't exist");
    }

    public void SetVariable(string name, object value)
    {
        _variables[name] = value;
    }
}
