using Maboroshi.Web.Models;

namespace Maboroshi.Web;

public interface IMockedRouteStore
{
    IEnumerable<MockedRoute> GetAll();
    MockedRoute? GetRouteByCriteria(string url, Models.HttpMethod method);
}
