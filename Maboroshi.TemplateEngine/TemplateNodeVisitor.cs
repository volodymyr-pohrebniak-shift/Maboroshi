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

        var metLastElse = false;
        var elseIndexes  = new List<int>();

        for (var i = 0; i < node.Body.Count; i++)
        {
            if (node.Body[i] is not FunctionNode func) continue;
            switch (func.Name)
            {
                case "else":
                    if (metLastElse)
                        throw new Exception("if block can't have multiple 'else'");

                    if (func.Parameters.Count == 0)
                    {
                        metLastElse = true;
                        elseIndexes.Add(i);
                    }
                    else
                    {
                        throw new Exception("'else' can't have any parameters");
                    }
                    break;
                case "elsif":
                    if (func.Parameters.Count == 0)
                    {
                        throw new Exception("'elsif' must have a parameter");
                    }
                    elseIndexes.Add(i);
                    break;
            }
        }

        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));

        var val = evaluatedParameters[0] switch
        {
            BoolReturn bVal => bVal.Value,
            StringReturn stringVal => !string.IsNullOrEmpty(stringVal),
            ArrayReturn<ReturnType> arrayVal => arrayVal.Values.Length > 0,
            _ => false,
        };

        if (val)
        {
            if (elseIndexes.Count == 0)
            {
                var sb = new StringBuilder();
                foreach (var body in node.Body)
                {
                    _context.InitializeScope();

                    var a = body.Accept(this);
                    sb.Append(body.Accept(this));
                    _context.ReleaseScope();
                }

                return sb.ToString();
            }
            else
            {
                var sb = new StringBuilder();
                foreach (var body in node.Body.Take(elseIndexes[0]))
                {
                    _context.InitializeScope();

                    sb.Append(body.Accept(this).GetValue());
                    _context.ReleaseScope();

                    return sb.ToString();
                }
            }
        }
        else
        {
            if (elseIndexes.Count == 0) return string.Empty;

            for (var i = 0; i < elseIndexes.Count - 1; i++)
            {
                var clause = node.Body[elseIndexes[i]] as FunctionNode;
                evaluatedParameters = clause!.Parameters.ConvertAll(x => x.Accept(this));

                val = evaluatedParameters[0] switch
                {
                    BoolReturn bVal => bVal.Value,
                    StringReturn stringVal => !string.IsNullOrEmpty(stringVal),
                    ArrayReturn<ReturnType> arrayVal => arrayVal.Values.Length > 0,
                    _ => false,
                };

                if (val)
                {
                    var sb1 = new StringBuilder();
                    foreach (var body in node.Body.Skip(elseIndexes[i] + 1).Take(elseIndexes[i + 1] - elseIndexes[i] - 1))
                    {
                        _context.InitializeScope();
                        sb1.Append(body.Accept(this));
                        _context.ReleaseScope();
                    }

                    return sb1.ToString();
                }
            }

            var sb = new StringBuilder();
            foreach (var body in node.Body.Skip(elseIndexes[^1] + 1))
            {
                _context.InitializeScope();
                sb.Append(body.Accept(this));
                _context.ReleaseScope();

                return sb.ToString();
            }
        }

        return string.Empty;
    }

    private StringReturn EvaluateRepeatBlock(BlockNode node)
    {
        if (node.Parameters.Count == 0)
            throw new Exception("repeat block should have at least one parameter");
        var evaluatedParameters = node.Parameters.ConvertAll(x => x.Accept(this));
        var repeatCount = -1;
        if (evaluatedParameters[0] is StringReturn minCountStr && int.TryParse(minCountStr.Value, out var minCount))
        {
            if (evaluatedParameters.Count > 1)
            {
                if (evaluatedParameters[0] is StringReturn maxCountStr && int.TryParse(maxCountStr.Value, out var maxCount))
                {
                    repeatCount = new Random().Next(minCount, maxCount + 1);
                }
                else
                {
                    throw new Exception("repeat block parameters should be int");
                }
            }
            else
            {
                repeatCount = minCount;
            }
            var sb = new StringBuilder();
            _context.InitializeScope();
            for (var i = 0; i < repeatCount; i++)
            {
                _context.SetVariable("index", new StringReturn(i.ToString()));
                foreach (var body in node.Body)
                {
                    _context.InitializeScope();
                    sb.Append(body.Accept(this).GetValue());
                    _context.ReleaseScope();
                }
            }
            _context.ReleaseScope();
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
        _context.InitializeScope();
        for (var i = 0; i < array.Values.Length; i++)
        {
            _context.SetVariable("this", array.Values[i]);
            _context.SetVariable("index", new StringReturn(i.ToString()));
            foreach (var body in node.Body)
            {
                _context.InitializeScope();
                sb.Append(body.Accept(this).GetValue());
                _context.ReleaseScope();
            }
        }
        _context.ReleaseScope();
        return new StringReturn(sb.ToString());
    }
}
