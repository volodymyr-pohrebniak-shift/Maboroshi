using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class StringsFunctionResolver : IFunctionResolver
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        // keeps non-ASCII text readable, the output is still valid JSON
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "lowercase" => LowerCase(additionalArguments),
            "uppercase" => UpperCase(additionalArguments),
            "includes" => Includes(additionalArguments),
            "substr" => Substr(additionalArguments),
            "split" => Split(additionalArguments),
            "padstart" => PadStart(additionalArguments),
            "json" => Json(additionalArguments),
            _ => null
        };
    }

    private static StringReturn LowerCase(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetString(out var str))
            throw new TemplateRuntimeException("Lowercase function requires a string parameter");
        return str.ToLowerInvariant();
    }

    private static StringReturn UpperCase(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetString(out var str))
            throw new TemplateRuntimeException("Uppercase function requires a string parameter");
        return str.ToUpperInvariant();
    }

    private static BoolReturn Includes(ReturnType[] arguments)
    {
        if (arguments.Length < 2)
            throw new TemplateRuntimeException("Includes function requires two parameters (string, searchValue)");
        if (!arguments[0].TryGetString(out var str) || !arguments[1].TryGetString(out var search))
            throw new TemplateRuntimeException("Both parameters must be strings");

        return str.Contains(search);
    }

    private static StringReturn Substr(ReturnType[] arguments)
    {
        if (arguments.Length < 2 || !arguments[0].TryGetString(out var str))
            throw new TemplateRuntimeException("Substr requires at least two parameters: (string, startIndex)");

        if (!arguments[1].TryGetInt(out var startIndex))
            throw new TemplateRuntimeException("Start index must be an integer");

        int length = str.Length - startIndex; // Default length: rest of the string

        if (arguments.Length > 2 && arguments[2].TryGetInt(out var parsedLength))
        {
            length = parsedLength;
        }

        return str.Substring(startIndex, Math.Min(length, str.Length - startIndex));
    }

    private static ArrayReturn<StringReturn> Split(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetString(out var str))
            throw new TemplateRuntimeException("Split requires at least one parameter (string)");

        string separator = arguments.Length > 1 && arguments[1].TryGetString(out var sep) ? sep : " ";

        return new ArrayReturn<StringReturn>(str.Split(separator).Select(x => new StringReturn(x)).ToArray());
    }

    private static StringReturn PadStart(ReturnType[] arguments)
    {
        if (arguments.Length < 2 || !arguments[0].TryGetString(out var str))
            throw new TemplateRuntimeException("PadStart requires at least two parameters (string, length)");

        if (!arguments[1].TryGetInt(out var totalLength))
            throw new TemplateRuntimeException("Length must be an integer");

        var padChar = arguments.Length > 2 && arguments[2].TryGetString(out var padStr) && padStr.Length > 0
            ? padStr[0]
            : '0';

        return str.PadLeft(totalLength, padChar);
    }

    private static StringReturn Json(ReturnType[] arguments)
    {
        if (arguments.Length == 0)
            throw new TemplateRuntimeException("Json function requires a parameter");

        return JsonSerializer.Serialize(ToJsonValue(arguments[0]), JsonOptions);
    }

    private static object ToJsonValue(ReturnType value) =>
        value.TryGetArray(out var items) ? items.Select(ToJsonValue).ToArray() : value.GetValue();
}
