using Maboroshi.TemplateEngine.FunctionResolvers;
using Maboroshi.TemplateEngine.TemplateNodes;

namespace Maboroshi.TemplateEngine;

public static class TemplateGenerator
{
    private static readonly IFunctionResolver[] StaticResolvers = [
        new StringsFunctionResolver(),
        new ArraysFunctionResolver(),
        new MathFunctionResolver(),
        new DateFunctionResolver(),
        new FakerFunctionResolver(new StaticFakerAdapter())
    ];

    public static Template CreateTemplate(string templateStr, TemplateCompilationOptions? options = null)
    {
        options ??= new TemplateCompilationOptions(false);
        try
        {
            var lexer = new Lexer(templateStr);
            var nodes = new Parser([.. lexer.Tokenize()], options.MaxDepth).Parse();

            return nodes.Count == 0 || (nodes.Count == 1 && nodes.First() is TextNode) ?
                new StaticTemplate(templateStr) :
                new Template(nodes, StaticResolvers, options);
        } catch (TemplateParsingException)
        {
            if (!options.Strict)
                return new StaticTemplate(templateStr);
            else throw;
        }
    }
}

// MaxIterations limits all repeat/each iterations of one render together, so nested loops can't multiply past it;
// MaxOutputLength is in characters, it limits the output and also every single value a template builds (variables included);
// MaxDepth limits nesting of blocks, sub-expressions and arrays: parsing and rendering are recursive,
// and a stack overflow can't be caught, it terminates the whole process
public record TemplateCompilationOptions(
    bool Strict,
    int MaxIterations = 10_000,
    int MaxOutputLength = TemplateCompilationOptions.DefaultMaxOutputLength,
    int MaxDepth = TemplateCompilationOptions.DefaultMaxDepth)
{
    // functions that allocate by an argument (padStart, join, faker words) don't see the options,
    // so they check the argument against this default before allocating
    public const int DefaultMaxOutputLength = 5 * 1024 * 1024;
    public const int DefaultMaxDepth = 100;

    // all values one render builds together, the intermediate ones too
    internal long MaxValuesSize => MaxOutputLength * 10L;
}