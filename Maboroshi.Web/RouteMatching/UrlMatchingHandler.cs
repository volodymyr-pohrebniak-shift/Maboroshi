using System.Collections.Concurrent;

namespace Maboroshi.Web.RouteMatching;

internal class UrlMatchingHandler(UrlMatchinStrategyFactory strategyFactory) : IUrlMatchingHandler
{
    private readonly ConcurrentDictionary<string, IUrlMatchingStrategy> _urlMatchingStrategies = [];
    
    public bool MatchesRoute(string template, string url)
    {
        var strategy = _urlMatchingStrategies.GetOrAdd(template, strategyFactory.Create);
        return strategy.IsUrlMatch(url);
    }
}