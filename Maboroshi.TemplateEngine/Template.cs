using System.Text;

namespace Maboroshi.TemplateEngine;

public class Template(string content)
{
    public string Content { get; } = content;

    public string Compile()
    {
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
        switch(node) {
            case TextNode textNode:
                return textNode.Value;
            case ExpressionNode expressionNode:
                return VisitExpression(expressionNode, context).ToString()!;
            case BlockNode blockNode:
                return VisitBlock(blockNode, context);
            default:
                throw new InvalidOperationException($"No visitor for node type {node.GetType()}");
        }

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
        else if (firstNode is IdentifierNode identifierNode)
        {
            return EvaluateIdentifier(identifierNode, node.Parameters.Skip(1).ToList(), context);
        } else
        {
            throw new Exception("Non-block expressions have to start with either identifier or variable");
        }
    }

    private object EvaluateIdentifier(IdentifierNode node, List<TemplateNode> parameters, TemplateContext context)
    {
        switch(node.Value.ToLower())
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
                break;

            default:
                throw new Exception($"Unknown identifier {node.Value}");
        }

        return string.Empty;
    }

    private object EvaluateParameter(TemplateNode node, TemplateContext context)
    {
        switch(node)
        {
            case ExpressionNode subExpressionNode:
                return VisitExpression(subExpressionNode, context);
            case LiteralNode literalNode:
                return literalNode.Value;
            case VariableNode variableNode:
                return context.GetVariable(variableNode.Value);
            default:
                throw new Exception("Wrong type for the parameter");
        }
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
                    for(var i = 0; i < repeatCount; i++)
                    {
                        foreach(var body in node.Body)
                        {
                            sb.Append(Visit(body, context));
                        }
                    }
                    return sb.ToString();
                } else
                {
                    throw new Exception($"repeat block parameters should be int");
                }
                

                break;
            default:
                return string.Empty;
        }
        return string.Empty;
    }
}

public class TemplateContext
{
    private readonly Dictionary<string, object> _variables = new();

    public object GetVariable(string name)
    {
        if (_variables.ContainsKey(name))
            return _variables[name];

        throw new Exception($"Variable {name} doesn't exist");
    }

    public void SetVariable(string name, object value)
    {
        _variables[name] = value;
    }
}
