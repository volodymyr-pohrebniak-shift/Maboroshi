using Maboroshi.Web.Models;

namespace Maboroshi.Web;

public interface IMockedRouteStore
{
    void SetRoutes(IEnumerable<MockedRoute> routes);
    IEnumerable<MockedRoute> GetAll();
    MockedRoute? GetRouteByCriteria(string url, Models.HttpMethod method);
}
