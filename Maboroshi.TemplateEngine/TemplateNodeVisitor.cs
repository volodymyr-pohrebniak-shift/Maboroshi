using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Text;

namespace Maboroshi.TemplateEngine;

internal class TemplateNodeVisitor(TemplateContext context, IEnumerable<IFunctionResolver> functionResolvers) : ITemplateNodeVisitor<ReturnType>
{
    private readonly TemplateContext _context = context;
    private readonly IEnumerable<IFunctionResolver> _functionResolvers = functionResolvers;

    public ReturnType VisitFunctionNode(FunctionNode node)
    {
        var parameters = node.Parameters.Select(p => p.Accept(this)).ToArray();

        ReturnType? result = null;

        foreach (var resolver in _functionResolvers)
        {
            result = resolver.TryResolve(node.Name.ToLowerInvariant(), parameters);
            if (result is not null)
                break;
        }

        if (result is null)
        {
            throw new Exception($"function {node.Name} is not defined");
        }

        return result;
    }

    public ReturnType VisitLiteralNode(LiteralNode node)
    {
        return new StringReturn(node.Value);
    }

    public ReturnType VisitTextNode(TextNode node)
    {
        return new StringReturn(node.Value);
    }

    public ReturnType VisitVariableNode(VariableNode node)
    {
        return _context.GetVariable(node.Value);
    }

    public ReturnType VisitBlockNode(BlockNode node)
    {
        return node.Name switch
        {
            "repeat" => EvaluateRepeatBlock(node),
            "each" => EvaluateEachBlock(node),
            "if" => EvaluateIfBlock(node),
            _ => new StringReturn(string.Empty),
        };
    }

    private StringReturn EvaluateIfBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new Exception("if block should have one parameter");

        var branches = new List<(TemplateNode? Condition, int Start, int End)>();
        TemplateNode? condition = node.Parameters[0];
        var start = 0;

        for (var i = 0; i < node.Body.Count; i++)
        {
            if (node.Body[i] is not FunctionNode { Name: "else" or "elsif" } clause) continue;

            if (condition is null)
                throw new Exception("'else' must be the last clause of an if block");

            branches.Add((condition, start, i));
            start = i + 1;

            if (clause.Name == "else")
            {
                if (clause.Parameters.Count != 0)
                    throw new Exception("'else' can't have any parameters");
                condition = null;
            }
            else
            {
                if (clause.Parameters.Count == 0)
                    throw new Exception("'elsif' must have a parameter");
                condition = clause.Parameters[0];
            }
        }

        branches.Add((condition, start, node.Body.Count));

        foreach (var (branchCondition, from, to) in branches)
        {
            if (branchCondition is null || IsTruthy(branchCondition.Accept(this)))
                return RenderInScope(node.Body.GetRange(from, to - from));
        }

        return string.Empty;
    }

    private static bool IsTruthy(ReturnType value) => value switch
    {
        BoolReturn b => b.Value,
        NumberReturn n => n.Value != 0 && !double.IsNaN(n.Value),
        StringReturn s => s.Value is not ("" or "0") && !s.Value.Equals("false", StringComparison.OrdinalIgnoreCase),
        _ => value.GetValue() is Array { Length: > 0 }
    };

    // variables declared in a block body are visible to the following nodes of that body, but not outside of it
    private string RenderInScope(IEnumerable<TemplateNode> nodes)
    {
        var sb = new StringBuilder();
        _context.InitializeScope();
        foreach (var body in nodes)
        {
            sb.Append(body.Accept(this));
        }
        _context.ReleaseScope();

        return sb.ToString();
    }

    private StringReturn EvaluateRepeatBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new Exception("repeat block should have at least one parameter");
        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));
        var repeatCount = -1;
        string? separator = null;
        if (TryGetIntParameter(evaluatedParameters[0], out var minCount))
        {
            if (evaluatedParameters.Count >= 3) {
                if (evaluatedParameters[1] is StringReturn maxCountStr && int.TryParse(maxCountStr.Value, out var maxCount))
                {
                    repeatCount = new Random().Next(minCount, maxCount + 1);
                }
                else
                {
                    throw new Exception("repeat block max parameter should be number");
                }

                if (evaluatedParameters[2] is StringReturn separatorReturn)
                {
                    separator = separatorReturn.Value;
                }
            } else if (evaluatedParameters.Count == 2)
            {
                if (evaluatedParameters[1] is StringReturn strReturn)
                {
                    if (int.TryParse(strReturn.Value, out var maxCount))
                    {
                        repeatCount = new Random().Next(minCount, maxCount + 1);
                    } else
                    {
                        separator = strReturn.Value;
                        repeatCount = minCount;
                    }
                }
                else
                {
                    throw new Exception("repeat block parameters should be number or string");
                }
            } else
            {
                repeatCount = minCount;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < repeatCount; i++)
            {
                _context.InitializeScope();
                _context.SetVariable("index", new StringReturn(i.ToString()));
                foreach (var body in node.Body)
                {
                    sb.Append(body.Accept(this));
                }
                _context.ReleaseScope();

                if (i != repeatCount - 1 && !string.IsNullOrEmpty(separator))
                {
                    if (TrimEndAndTrackNewlines(sb))
                    {
                        sb.AppendLine(separator);
                    } else
                    {
                        sb.Append(separator);
                    }
                }
            }
            return new StringReturn(sb.ToString());
        }
        else
        {
            throw new Exception("repeat block parameters should be int");
        }
    }

    private StringReturn EvaluateEachBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new Exception("each block should have one parameter");
        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));

        if (evaluatedParameters[0] is not ArrayReturn<ReturnType> array)
            throw new Exception("each block should have a parameter of an array type");

        var sb = new StringBuilder();
        for (var i = 0; i < array.Values.Length; i++)
        {
            _context.InitializeScope();
            _context.SetVariable("this", array.Values[i]);
            _context.SetVariable("index", new StringReturn(i.ToString()));
            foreach (var body in node.Body)
            {
                sb.Append(body.Accept(this));
            }
            _context.ReleaseScope();
        }
        return new StringReturn(sb.ToString());
    }

    public static bool TrimEndAndTrackNewlines(StringBuilder sb)
    {
        if (sb == null || sb.Length == 0)
            return false;

        int i = sb.Length - 1;
        bool foundNewlineOrCR = false;

        while (i >= 0)
        {
            char c = sb[i];
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                if (c == '\n' || c == '\r')
                    foundNewlineOrCR = true;

                i--;
            }
            else
            {
                break;
            }
        }

        sb.Length = i + 1;
        return foundNewlineOrCR;
    }

    private static (string trimmed, bool hadNewlineOrCarriageReturn) TrimEndAndTrackNewlines(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (input, false);

        int end = input.Length - 1;
        bool foundNewlineOrCR = false;

        while (end >= 0)
        {
            char c = input[end];
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
            {
                if (c == '\n' || c == '\r')
                {
                    foundNewlineOrCR = true;
                }
                end--;
            }
            else
            {
                break;
            }
        }

        return (input.Substring(0, end + 1), foundNewlineOrCR);
    }

    private static bool TryGetIntParameter(ReturnType param, out int output)
    {
        if (param is NumberReturn n)
        {
            output = (int)n.Value;
            return true;
        } else if (param is StringReturn str && int.TryParse(str.Value, out output))
        {
            return true;
        }
        output = Int32.MinValue;
        return false;
    }
}
