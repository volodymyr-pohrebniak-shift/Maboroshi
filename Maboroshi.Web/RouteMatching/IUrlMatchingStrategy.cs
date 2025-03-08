namespace Maboroshi.Web.RouteMatching;

public interface IUrlMatchingStrategy
{
    bool IsUrlMatch(string url);
}