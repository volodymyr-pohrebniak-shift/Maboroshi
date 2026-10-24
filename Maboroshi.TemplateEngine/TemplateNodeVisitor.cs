using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Globalization;
using System.Text;

namespace Maboroshi.TemplateEngine;

internal class TemplateNodeVisitor(TemplateContext context, IEnumerable<IFunctionResolver> functionResolvers, bool strict) : ITemplateNodeVisitor<ReturnType>
{
    private readonly TemplateContext _context = context;
    private readonly IEnumerable<IFunctionResolver> _functionResolvers = functionResolvers;

    // in non-strict mode a node that fails at runtime is rendered as empty text instead of failing the whole template
    public string Render(TemplateNode node)
    {
        try
        {
            return node.Accept(this).ToString();
        }
        catch (TemplateRuntimeException) when (!strict)
        {
            return string.Empty;
        }
    }

    public ReturnType VisitFunctionNode(FunctionNode node)
    {
        var parameters = node.Parameters.Select(p => p.Accept(this)).ToArray();
        var name = node.Name.ToLowerInvariant();

        foreach (var resolver in _functionResolvers)
        {
            ReturnType? result;
            try
            {
                result = resolver.TryResolve(name, parameters);
            }
            catch (Exception e) when (e is not TemplateException)
            {
                throw new TemplateRuntimeException($"function {node.Name} failed: {e.Message}", e);
            }

            if (result is not null)
                return result;
        }

        throw new TemplateRuntimeException($"function {node.Name} is not defined");
    }

    public ReturnType VisitLiteralNode(LiteralNode node)
    {
        return node.IsNumber
            ? new NumberReturn(double.Parse(node.Value, CultureInfo.InvariantCulture))
            : new StringReturn(node.Value);
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
            throw new TemplateRuntimeException("if block should have one parameter");

        // each branch is a condition (null for 'else') and the range of body nodes it renders
        var branches = new List<(TemplateNode? Condition, int Start, int End)>();
        TemplateNode? condition = node.Parameters[0];
        var start = 0;

        for (var i = 0; i < node.Body.Count; i++)
        {
            if (node.Body[i] is not FunctionNode { Name: "else" or "elsif" } clause) continue;

            if (condition is null)
                throw new TemplateRuntimeException("'else' must be the last clause of an if block");

            branches.Add((condition, start, i));
            start = i + 1;

            if (clause.Name == "else")
            {
                if (clause.Parameters.Count != 0)
                    throw new TemplateRuntimeException("'else' can't have any parameters");
                condition = null;
            }
            else
            {
                if (clause.Parameters.Count == 0)
                    throw new TemplateRuntimeException("'elsif' must have a parameter");
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
        _ => value.TryGetArray(out var items) && items.Length > 0
    };

    // variables declared in a block body are visible to the following nodes of that body, but not outside of it
    private string RenderInScope(IEnumerable<TemplateNode> nodes, params (string Name, ReturnType Value)[] variables)
    {
        var sb = new StringBuilder();
        _context.InitializeScope();
        try
        {
            foreach (var (name, value) in variables)
            {
                _context.SetVariable(name, value);
            }

            foreach (var body in nodes)
            {
                sb.Append(Render(body));
            }
        }
        finally
        {
            _context.ReleaseScope();
        }

        return sb.ToString();
    }

    private StringReturn EvaluateRepeatBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new TemplateRuntimeException("repeat block should have at least one parameter");
        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));

        if (!evaluatedParameters[0].TryGetInt(out var minCount))
            throw new TemplateRuntimeException("repeat block parameters should be int");

        var repeatCount = minCount;
        string? separator = null;
        if (evaluatedParameters.Count >= 2)
        {
            if (evaluatedParameters[1].TryGetInt(out var maxCount))
            {
                if (maxCount < minCount)
                    throw new TemplateRuntimeException("repeat block max parameter should not be less than min");
                repeatCount = (int)Random.Shared.NextInt64(minCount, (long)maxCount + 1);
            }
            else if (evaluatedParameters.Count == 2)
            {
                separator = evaluatedParameters[1].ToString();
            }
            else
            {
                throw new TemplateRuntimeException("repeat block max parameter should be number");
            }

            if (evaluatedParameters.Count >= 3)
                separator = evaluatedParameters[2].ToString();
        }

        var sb = new StringBuilder();
        for (var i = 0; i < repeatCount; i++)
        {
            sb.Append(RenderInScope(node.Body, ("index", new NumberReturn(i))));

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

    private StringReturn EvaluateEachBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new TemplateRuntimeException("each block should have one parameter");
        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));

        if (!evaluatedParameters[0].TryGetArray(out var items))
            throw new TemplateRuntimeException("each block should have a parameter of an array type");

        var sb = new StringBuilder();
        for (var i = 0; i < items.Length; i++)
        {
            sb.Append(RenderInScope(node.Body, ("this", items[i]), ("index", new NumberReturn(i))));
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
}
