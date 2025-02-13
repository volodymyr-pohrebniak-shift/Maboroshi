using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;
using System.Text;

namespace Maboroshi.TemplateEngine;

internal class TemplateNodeVisitor(TemplateContext context, IEnumerable<IFunctionResolver> functionResolvers) : ITemplateNodeVisitor<ReturnType>
{
    private readonly TemplateContext _context = context;
    private readonly IEnumerable<IFunctionResolver> _functionResolvers = functionResolvers;

    public ReturnType VisitBlockNode(BlockNode node)
    {
        switch (node.Name)
        {
            case "repeat":
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
                            sb.Append(body.Accept(this));
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
            default:
                return new StringReturn(string.Empty);
        }
    }

    public ReturnType VisitFunctionNode(FunctionNode node)
    {
        var parameters = node.Parameters.Select(p => p.Accept(this)).ToArray();

        ReturnType? result = null;

        foreach (var resolver in _functionResolvers)
        {
            result = resolver.TryResolve(node.Name, parameters);
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
}
