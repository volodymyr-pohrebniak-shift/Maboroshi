using System.Collections.Concurrent;
using Maboroshi.Web.Models;
using Maboroshi.Web.RouteMatching;

namespace Maboroshi.Web;

public class InMemoryMockedRouteStore(IEnumerable<MockedRoute> routes, IUrlMatchingHandler urlMatchingHandler)
    : IMockedRouteStore
{
    private ConcurrentBag<MockedRoute> _routes = new(routes);

    public void SetRoutes(IEnumerable<MockedRoute> routes)
    {
        _routes = new ConcurrentBag<MockedRoute>(routes);
    }

    public MockedRoute? GetRouteByCriteria(string url, Models.HttpMethod method)
    {
        return _routes.FirstOrDefault(route => (route.HttpMethod & method) != 0 && urlMatchingHandler.MatchesRoute(route.UrlTemplate, url));
    }
}