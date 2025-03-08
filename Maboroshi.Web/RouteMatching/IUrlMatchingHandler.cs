namespace Maboroshi.Web.RouteMatching;

public interface IUrlMatchingHandler
{
    bool MatchesRoute(string template, string url);
}