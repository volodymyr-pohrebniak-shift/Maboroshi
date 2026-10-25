using Maboroshi.Web;
using Maboroshi.Web.Models;
using Maboroshi.Web.RouteMatching;
using HttpMethod = Maboroshi.Web.Models.HttpMethod;

namespace Maboroshi.UnitTests;

public class InMemoryMockedRouteStoreTests
{
    [Fact]
    public void GetRouteByCriteria_ShouldReturnFirstMatchingRoute_InConfiguredOrder()
    {
        var store = new InMemoryMockedRouteStore(new MatchEverything());
        store.SetRoutes([CreateRoute("/first"), CreateRoute("/second"), CreateRoute("/third")]);

        Assert.Equal("/first", store.GetRouteByCriteria("/any", HttpMethod.GET)!.UrlTemplate);
        Assert.Equal(["/first", "/second", "/third"], store.GetAll().Select(r => r.UrlTemplate));
    }

    [Fact]
    public void GetRouteByCriteria_ShouldSkipDisabledRoutes_AndRoutesForOtherMethods()
    {
        var store = new InMemoryMockedRouteStore(new MatchEverything());
        store.SetRoutes([CreateRoute("/disabled", enabled: false), CreateRoute("/post", HttpMethod.POST), CreateRoute("/get")]);

        Assert.Equal("/get", store.GetRouteByCriteria("/any", HttpMethod.GET)!.UrlTemplate);
    }

    private static MockedRoute CreateRoute(string urlTemplate, HttpMethod method = HttpMethod.GET, bool enabled = true) =>
        new(urlTemplate, method, [], ResponseSelectionStrategy.Default, enabled);

    private class MatchEverything : IUrlMatchingHandler
    {
        public bool MatchesRoute(string template, string url) => true;
    }
}
