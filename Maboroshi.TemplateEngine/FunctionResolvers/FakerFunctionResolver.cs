namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class FakerFunctionResolver(IFakerAdapter fakerAdapter) : IFunctionResolver
{
    private readonly IFakerAdapter _fakerAdapter = fakerAdapter;

    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "faker" => GetFakerValue(additionalArguments),
            "int" => _fakerAdapter.GetFakeValue("random.number", additionalArguments),
            "float" => _fakerAdapter.GetFakeValue("random.float", additionalArguments),
            "bool" => _fakerAdapter.GetFakeValue("random.bool", additionalArguments),
            _ => throw new NotImplementedException(),
        };
    }

    private ReturnType? GetFakerValue(ReturnType[] additionalArguments)
    {
        if (additionalArguments == null || additionalArguments.Length == 0)
        {
            throw new Exception("faker function requires a path");
        }

        if (additionalArguments[0] is StringReturn path)
            return _fakerAdapter.GetFakeValue(path, additionalArguments.Skip(1).ToArray());
        else
            throw new Exception("faker function requires a path");
    }
}
