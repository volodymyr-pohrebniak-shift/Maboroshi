using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Maboroshi.TemplateEngine;

// Template values are loosely typed: request data always comes as strings,
// so a numeric argument may be either a number or a string holding a number
internal static class ReturnTypeExtensions
{
    public static bool TryGetNumber(this ReturnType value, out double number)
    {
        switch (value)
        {
            case NumberReturn n:
                number = n.Value;
                return true;
            // "NaN" and "Infinity" are left as text, template numbers are always finite
            case StringReturn s when double.TryParse(s.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && double.IsFinite(number):
                return true;
            default:
                number = 0;
                return false;
        }
    }

    public static bool TryGetInt(this ReturnType value, out int number)
    {
        number = 0;
        if (!value.TryGetNumber(out var d) || d != Math.Floor(d) || d < int.MinValue || d > int.MaxValue)
            return false;

        number = (int)d;
        return true;
    }

    // any single value can be used as text, an array can't
    public static bool TryGetString(this ReturnType value, [NotNullWhen(true)] out string? text)
    {
        text = value.TryGetArray(out _) ? null : value.ToString();
        return text is not null;
    }

    // arrays of any element type are visible as ReturnType[] thanks to array covariance
    public static bool TryGetArray(this ReturnType value, [NotNullWhen(true)] out ReturnType[]? items)
    {
        items = value.GetValue() as ReturnType[];
        return items is not null;
    }
}
