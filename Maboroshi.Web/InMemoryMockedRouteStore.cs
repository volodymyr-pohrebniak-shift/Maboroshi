using System.Collections.Concurrent;
using Maboroshi.Web.Models;
using Maboroshi.Web.RouteMatching;

namespace Maboroshi.Web;

public class InMemoryMockedRouteStore(IUrlMatchingHandler urlMatchingHandler)
    : IMockedRouteStore
{
    private ConcurrentBag<MockedRoute> _routes = [];

    public void SetRoutes(IEnumerable<MockedRoute> routes)
    {
        _routes = [.. routes];
    }

    public MockedRoute? GetRouteByCriteria(string url, Models.HttpMethod method)
    {
        return _routes.FirstOrDefault(route => (route.HttpMethod & method) != 0 && urlMatchingHandler.MatchesRoute(route.UrlTemplate, url));
    }

    public IEnumerable<MockedRoute> GetAll()
    {
        return _routes;
    }
}