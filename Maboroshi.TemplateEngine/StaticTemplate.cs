using Maboroshi.TemplateEngine.FunctionResolvers;

namespace Maboroshi.TemplateEngine;

public class StaticTemplate : Template
{
    private readonly string _template;
    public StaticTemplate(string template)
    {
        _template = template;
    }

    public override string Compile(params IFunctionResolver[] additionalFunctionResolvers)
    {
        return _template;
    }
}