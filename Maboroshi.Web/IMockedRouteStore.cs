using Maboroshi.Web.Models;

namespace Maboroshi.Web;

public interface IMockedRouteStore
{
    MockedRoute? GetRouteByCriteria(string url, Models.HttpMethod method);
}
