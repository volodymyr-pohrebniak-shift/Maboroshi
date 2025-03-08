namespace Maboroshi.Web.RouteMatching;

internal class UrlMatchingHandler(UrlMatchinStrategyFactory strategyFactory) : IUrlMatchingHandler
{
    private readonly Dictionary<string, IUrlMatchingStrategy> _urlMatchingStrategies = [];
    
    public bool MatchesRoute(string template, string url)
    {
        if (_urlMatchingStrategies.TryGetValue(template, out var strategy))
        {
            return strategy.IsUrlMatch(url);
        }

        strategy = strategyFactory.Create(template);
        _urlMatchingStrategies[template] = strategy;
        return strategy.IsUrlMatch(url);
    }
}