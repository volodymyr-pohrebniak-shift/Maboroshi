namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class VariablesFunctionResolver(TemplateContext context) : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "var" => Var(additionalArguments),
            _ => null
        };
    }

    private StringReturn Var(ReturnType[] parameters)
    {
        if (parameters.Length < 2 || parameters[0] is not StringReturn str)
            throw new Exception("Var function should have two parameters");

        context.SetVariable(str, parameters[1]);

        return string.Empty;
    }
}
