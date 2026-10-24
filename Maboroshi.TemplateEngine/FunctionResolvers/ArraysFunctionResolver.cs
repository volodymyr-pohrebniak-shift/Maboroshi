namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class ArraysFunctionResolver : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName.ToLower() switch
        {
            "array" => Array(additionalArguments),
            "oneof" => OneOf(additionalArguments),
            "someof" => SomeOf(additionalArguments),
            "join" => Join(additionalArguments),
            "slice" => Slice(additionalArguments),
            "sort" => Sort(additionalArguments),
            "reverse" => Reverse(additionalArguments),
            _ => null
        };
    }

    private static ArrayReturn<ReturnType> Array(ReturnType[] arguments)
    {
        if (arguments.Length == 0)
            throw new TemplateRuntimeException("at least one item should be provided for an array");
        return new ArrayReturn<ReturnType>(arguments);
    }

    private static ReturnType OneOf(ReturnType[] arguments) {
        if (arguments.Length > 0 && arguments[0].TryGetArray(out var items) && items.Length > 0)
        {
            return items[Random.Shared.Next(0, items.Length)];
        }
        throw new TemplateRuntimeException("oneOf function requires a non-empty array parameter");
    }

    private static ArrayReturn<ReturnType> SomeOf(ReturnType[] arguments)
    {
        if (arguments.Length < 3 || !arguments[0].TryGetArray(out var items) ||
            !arguments[1].TryGetInt(out var min) || !arguments[2].TryGetInt(out var max) || min > max)
        {
            throw new TemplateRuntimeException("someOf requires (array, min, max, [stringify]) parameters.");
        }

        var stringify = arguments.Length > 3 && arguments[3] is StringReturn str && bool.TryParse(str.Value, out var result) && result;

        var count = (int)Random.Shared.NextInt64(min, (long)max + 1);
        var selected = items.OrderBy(_ => Guid.NewGuid()).Take(count).ToArray();

        return stringify
            ? new ArrayReturn<ReturnType>([.. selected.Select(item => new StringReturn(item.ToString()))])
            : new ArrayReturn<ReturnType>(selected);
    }

    private static StringReturn Join(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetArray(out var items))
        {
            throw new TemplateRuntimeException("join function requires an array parameter.");
        }

        var separator = arguments.Length > 1 && arguments[1].TryGetString(out var sep) ? sep : " ";
        return new StringReturn(string.Join(separator, items.Select(v => v.ToString())));
    }

    private static ArrayReturn<ReturnType> Slice(ReturnType[] arguments)
    {
        if (arguments.Length < 3 || !arguments[0].TryGetArray(out var items) ||
            !arguments[1].TryGetInt(out var start) || !arguments[2].TryGetInt(out var end))
        {
            throw new TemplateRuntimeException("slice function requires (array, start, end) parameters.");
        }

        return new ArrayReturn<ReturnType>(items.Skip(start).Take(end - start).ToArray());
    }

    private static ArrayReturn<ReturnType> Sort(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetArray(out var items))
        {
            throw new TemplateRuntimeException("sort function requires an array parameter.");
        }

        string order = arguments.Length > 1 && arguments[1] is StringReturn str ? str.Value.ToLowerInvariant() : "asc";

        // numbers are sorted by value, anything else as text independently of the server culture
        var sortedValues = items.All(v => v.TryGetNumber(out _))
            ? items.OrderBy(AsNumber).ToArray()
            : items.OrderBy(v => v.ToString(), StringComparer.Ordinal).ToArray();

        return order == "desc" ? new ArrayReturn<ReturnType>([.. sortedValues.Reverse()]) : new ArrayReturn<ReturnType>(sortedValues);
    }

    private static ArrayReturn<ReturnType> Reverse(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || !arguments[0].TryGetArray(out var items))
        {
            throw new TemplateRuntimeException("reverse function requires an array parameter.");
        }

        return new ArrayReturn<ReturnType>([.. items.Reverse()]);
    }

    private static double AsNumber(ReturnType value) => value.TryGetNumber(out var number) ? number : double.NaN;
}
