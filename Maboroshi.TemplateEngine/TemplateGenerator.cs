using Maboroshi.TemplateEngine.FunctionResolvers;

namespace Maboroshi.TemplateEngine;

public sealed class TemplateGenerator
{
    private static readonly IFunctionResolver[] StaticResolvers = [
        new StringsFunctionResolver(),
        new ArraysFunctionResolver(),
        new FakerFunctionResolver(new StaticFakerAdapter())
    ];

    public Template CreateTemplate(string templateStr)
    {
        // TODO check if templateStr doesn't contain expression
        // TODO handle exceptions
        var lexer = new Lexer(templateStr);
        var nodes = new Parser(lexer.Tokenize().ToList()).Parse();

        return new Template(nodes, StaticResolvers);
    }
}

public record TemplateCompilationOptions(bool Strict);