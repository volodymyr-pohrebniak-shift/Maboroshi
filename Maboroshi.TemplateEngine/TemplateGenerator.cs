using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

public static class TemplateGenerator
{
    private static readonly IFunctionResolver[] StaticResolvers = [
        new StringsFunctionResolver(),
        new ArraysFunctionResolver(),
        new FakerFunctionResolver(new StaticFakerAdapter())
    ];

    public static Template CreateTemplate(string templateStr, TemplateCompilationOptions? options = null)
    {
        options ??= new TemplateCompilationOptions(false);
        try
        {
            var lexer = new Lexer(templateStr);
            var nodes = new Parser([.. lexer.Tokenize()]).Parse();

            return nodes.Count == 0 || (nodes.Count == 1 && nodes.First() is TextNode) ?
                new StaticTemplate(templateStr) :
                new Template(nodes, StaticResolvers);
        } catch (TemplateParsingException)
        {
            if (!options.Strict)
                return new StaticTemplate(templateStr);
            else throw;
        }
    }
}

public record TemplateCompilationOptions(bool Strict);