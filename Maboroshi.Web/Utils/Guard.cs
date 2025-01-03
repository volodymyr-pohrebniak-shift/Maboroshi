namespace Maboroshi.Web.Utils;

public interface IGuard
{
}

public class Guard : IGuard
{
    public static Guard Against { get; } = new Guard();

    private Guard() { }
}

public static class GuardExtensions
{
    public static T Null<T>(this Guard guard, T? input, string? parameterName = null)
    {
        if (input is null)
        {
            throw new ArgumentNullException(parameterName);
        }
        return input;
    }

    public static string NullOrWhiteSpace(this Guard guard, string? str, string? parameterName = null)
    {
        Guard.Against.Null(str, parameterName);

        if (str == string.Empty)
        {
            throw new ArgumentNullException(parameterName);
        }

        return str!;
    }

    public static IEnumerable<T> NullOrEmpty<T>(this Guard guard, IEnumerable<T>? collection, string? parameterName = null)
    {
        Guard.Against.Null(collection, parameterName);

        if ((collection is Array and { Length: 0 }) || (collection!.TryGetNonEnumeratedCount(out var count) && count == 0))
        {
            throw new ArgumentNullException(parameterName);
        }

        return collection!;
    }
}