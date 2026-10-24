namespace Maboroshi.TemplateEngine.FunctionResolvers;

internal class MathFunctionResolver : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName switch
        {
            "add" => Add(additionalArguments),
            "subtract" => Subtract(additionalArguments),
            "multiply" => Multiply(additionalArguments),
            "divide" => Divide(additionalArguments),
            "modulo" or "mod" => Modulo(additionalArguments),
            "ceil" => Ceil(additionalArguments),
            "floor" => Floor(additionalArguments),
            "round" => Round(additionalArguments),
            "tofixed" => ToFixed(additionalArguments),
            "eq" => Eq(additionalArguments),
            "noteq" => new BoolReturn(!Eq(additionalArguments)),
            "gt" => Cmp(additionalArguments, (a, b) => a > b),
            "gte" => Cmp(additionalArguments, (a, b) => a >= b),
            "lt" => Cmp(additionalArguments, (a, b) => a < b),
            "lte" => Cmp(additionalArguments, (a, b) => a <= b),
            _ => null
        };
    }

    private static double? ExtractNumber(ReturnType arg) => arg.TryGetNumber(out var value) ? value : null;

    private static NumberReturn Add(ReturnType[] args) =>
        args.Select(ExtractNumber).Where(n => n.HasValue).Sum(n => n!.Value);

    private static NumberReturn Subtract(ReturnType[] args)
    {
        var numbers = args.Select(ExtractNumber).Where(n => n.HasValue).Select(n => n.Value).ToList();
        if (numbers.Count == 0) return 0;
        return numbers.Skip(1).Aggregate(numbers[0], (acc, val) => acc - val);
    }

    private static NumberReturn Multiply(ReturnType[] args) =>
        new(args.Select(ExtractNumber).Where(n => n.HasValue).Aggregate(1.0, (acc, val) => acc * val!.Value));

    private static NumberReturn Divide(ReturnType[] args)
    {
        var numbers = args.Select(ExtractNumber).Where(n => n.HasValue).Select(n => n.Value).ToList();
        if (numbers.Count == 0) return new(0);
        return new(numbers.Skip(1).Aggregate(numbers[0], (acc, val) => acc / val));
    }

    private static NumberReturn Modulo(ReturnType[] args)
    {
        var nums = args.Select(ExtractNumber).Where(n => n.HasValue).Select(n => n.Value).ToList();
        if (nums.Count < 2) throw new TemplateRuntimeException("Modulo requires at least two numbers");
        return new(nums[0] % nums[1]);
    }

    private static NumberReturn Ceil(ReturnType[] args)
    {
        var val = ExtractNumber(args.FirstOrDefault() ?? throw new TemplateRuntimeException("Missing argument"));
        if (!val.HasValue) throw new TemplateRuntimeException("Invalid number");
        return new(Math.Ceiling(val.Value));
    }

    private static NumberReturn Floor(ReturnType[] args)
    {
        var val = ExtractNumber(args.FirstOrDefault() ?? throw new TemplateRuntimeException("Missing argument"));
        if (!val.HasValue) throw new TemplateRuntimeException("Invalid number");
        return new(Math.Floor(val.Value));
    }

    private static NumberReturn Round(ReturnType[] args)
    {
        var val = ExtractNumber(args.FirstOrDefault() ?? throw new TemplateRuntimeException("Missing argument"));
        if (!val.HasValue) throw new TemplateRuntimeException("Invalid number");
        return new(Math.Round(val.Value));
    }

    private static NumberReturn ToFixed(ReturnType[] args)
    {
        if (args.Length < 2) throw new TemplateRuntimeException("toFixed requires value and decimal places");
        var number = ExtractNumber(args[0]);
        var decimals = (int?) ExtractNumber(args[1]);
        if (!number.HasValue || !decimals.HasValue) throw new TemplateRuntimeException("Invalid arguments");
        return new(Math.Round(number.Value, decimals.Value));
    }

    // request data always comes as strings, so values that look like numbers are compared as numbers, anything else as text
    private static BoolReturn Eq(ReturnType[] args)
    {
        if (args.Length < 2) throw new TemplateRuntimeException("Comparison requires at least 2 arguments");

        return args[0].TryGetNumber(out var first) && args[1].TryGetNumber(out var second)
            ? first == second
            : args[0].ToString() == args[1].ToString();
    }

    // like in JavaScript, comparing something that is not a number gives false
    private static BoolReturn Cmp(ReturnType[] args, Func<double, double, bool> compare)
    {
        if (args.Length < 2) throw new TemplateRuntimeException("Comparison requires at least 2 arguments");
        for (int i = 1; i < args.Length; i++)
        {
            if (!args[i - 1].TryGetNumber(out var previous) || !args[i].TryGetNumber(out var current) || !compare(previous, current))
                return false;
        }
        return true;
    }
}
