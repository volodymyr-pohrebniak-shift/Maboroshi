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
            throw new Exception("at least one item should be provided for an array");
        return new ArrayReturn<ReturnType>(arguments);
    }

    private static ReturnType OneOf(ReturnType[] arguments) {
        if (arguments.Length > 0 && arguments[0] is ArrayReturn<ReturnType> array)
        {
            var index = new Random().Next(0, array.Values.Length);
            return array.Values[index];
        }
        throw new Exception("oneOf function requires an array parameter");
    }

    private static ArrayReturn<ReturnType> SomeOf(ReturnType[] arguments)
    {
        if (arguments.Length < 3 || arguments[0] is not ArrayReturn<ReturnType> array ||
            arguments[1] is not StringReturn minStr || !int.TryParse(minStr.Value, out int min) ||
            arguments[2] is not StringReturn maxStr || !int.TryParse(maxStr.Value, out int max))
        {
            throw new Exception("someOf requires (array, min, max, [stringify]) parameters.");
        }

        var stringify = arguments.Length > 3 && arguments[3] is StringReturn str && bool.TryParse(str.Value, out var result) && result;

        var count = new Random().Next(min, max + 1);
        var selected = array.Values.OrderBy(_ => Guid.NewGuid()).Take(count).ToArray();

        return stringify
            ? new ArrayReturn<ReturnType>(selected
                .Select(item => new StringReturn(item.GetValue().ToString()!))
                .ToArray<ReturnType>())
            : new ArrayReturn<ReturnType>(selected);
    }

    private static StringReturn Join(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not ArrayReturn<ReturnType> array)
        {
            throw new Exception("join function requires an array parameter.");
        }

        var separator = arguments.Length > 1 && arguments[1] is StringReturn sep ? sep.Value : " ";
        return new StringReturn(string.Join(separator, array.Values.Select(v => v.GetValue().ToString())));
    }

    private static ArrayReturn<ReturnType> Slice(ReturnType[] arguments)
    {
        if (arguments.Length < 3 || arguments[0] is not ArrayReturn<ReturnType> array ||
            arguments[1] is not StringReturn startStr || !int.TryParse(startStr.Value, out int start) ||
            arguments[2] is not StringReturn endStr || !int.TryParse(endStr.Value, out int end))
        {
            throw new Exception("slice function requires (array, start, end) parameters.");
        }

        return new ArrayReturn<ReturnType>(array.Values.Skip(start).Take(end - start).ToArray());
    }

    private static ArrayReturn<ReturnType> Sort(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not ArrayReturn<ReturnType> array)
        {
            throw new Exception("sort function requires an array parameter.");
        }

        string order = arguments.Length > 1 && arguments[1] is StringReturn str ? str.Value.ToLower() : "asc";
        var sortedValues = array.Values.OrderBy(v => v.GetValue().ToString()).ToArray();

        return order == "desc" ? new ArrayReturn<ReturnType>(sortedValues.Reverse().ToArray()) : new ArrayReturn<ReturnType>(sortedValues);
    }

    private static ArrayReturn<ReturnType> Reverse(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not ArrayReturn<ReturnType> array)
        {
            throw new Exception("reverse function requires an array parameter.");
        }

        return new ArrayReturn<ReturnType>(array.Values.Reverse().ToArray());
    }

}
