using Maboroshi.TemplateEngine.FunctionResolvers;

namespace Maboroshi.TemplateEngine;

public static class TemplateEngine
{
    private static readonly IFunctionResolver[] _staticResolvers = [
        new StringsFunctionResolver()
    ];

    public static Template CreateTemplate(string templateStr)
    {
        // TODO check if templateStr doesn't contain expression
        // TODO handle exceptions
        var lexer = new Lexer(templateStr);
        var nodes = new Parser(lexer.Tokenize().ToList()).Parse();

        return new Template(nodes, templateStr, _staticResolvers);
    }
}

public record TemplateCompilationOptions(bool Strict);