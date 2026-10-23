using System.Globalization;

namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class DateFunctionResolver : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "now" => Now(additionalArguments),
            _ => null
        };
    }

    // {{now}} -> ISO 8601 UTC, {{now 'yyyy-MM-dd'}} -> custom .NET format
    private static StringReturn Now(ReturnType[] arguments)
    {
        var format = arguments.Length > 0 && arguments[0] is StringReturn str ? str.Value : "o";
        return DateTime.UtcNow.ToString(format, CultureInfo.InvariantCulture);
    }
}
