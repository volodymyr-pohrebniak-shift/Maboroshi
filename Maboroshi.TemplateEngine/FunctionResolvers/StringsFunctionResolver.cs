namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class StringsFunctionResolver : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "lowercase" => LowerCase(additionalArguments),
            "uppercase" => UpperCase(additionalArguments),
            "includes" => Includes(additionalArguments),
            "substr" => Substr(additionalArguments),
            "split" => Split(additionalArguments),
            "padStart" => PadStart(additionalArguments),
            _ => null
        };
    }

    private static StringReturn LowerCase(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not StringReturn str)
            throw new Exception("Lowercase function requires a string parameter");
        return str.Value.ToLower();
    }

    private static StringReturn UpperCase(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not StringReturn str)
            throw new Exception("Uppercase function requires a string parameter");
        return str.Value.ToUpper();
    }

    private static StringReturn Includes(ReturnType[] arguments)
    {
        if (arguments.Length < 2)
            throw new Exception("Includes function requires two parameters (string, searchValue)");
        if (arguments[0] is not StringReturn str || arguments[1] is not StringReturn search)
            throw new Exception("Both parameters must be strings");

        return str.Value.Contains(search).ToString();
    }

    private static StringReturn Substr(ReturnType[] arguments)
    {
        if (arguments.Length < 2 || arguments[0] is not StringReturn str || arguments[1] is not StringReturn startStr)
            throw new Exception("Substr requires at least two parameters: (string, startIndex)");

        if (!int.TryParse(startStr, out int startIndex))
            throw new Exception("Start index must be an integer");

        int length = str.Value.Length - startIndex; // Default length: rest of the string

        if (arguments.Length > 2 && arguments[2] is StringReturn lengthStr && int.TryParse(lengthStr, out int parsedLength))
        {
            length = parsedLength;
        }

        return str.Value.Substring(startIndex, Math.Min(length, str.Value.Length - startIndex));
    }

    private static ArrayReturn<StringReturn> Split(ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not StringReturn str)
            throw new Exception("Split requires at least one parameter (string)");

        string separator = arguments.Length > 1 && arguments[1] is StringReturn sep ? sep : " ";

        return new ArrayReturn<StringReturn>(str.Value.Split(separator).Select(x => new StringReturn(x)).ToArray());
    }

    private static StringReturn PadStart(ReturnType[] arguments)
    {
        if (arguments.Length < 2 || arguments[0] is not StringReturn str || arguments[1] is not StringReturn lengthStr)
            throw new Exception("PadStart requires at least two parameters (string, length)");

        if (!int.TryParse(lengthStr, out int totalLength))
            throw new Exception("Length must be an integer");

        var padChar = arguments.Length > 2 && arguments[2] is StringReturn padStr && padStr.Value.Length > 0
            ? padStr.Value[0]
            : '0';

        return str.Value.PadLeft(totalLength, padChar);
    }
}