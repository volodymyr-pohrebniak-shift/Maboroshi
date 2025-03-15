using Maboroshi.Web.Models;

namespace Maboroshi.Web.Templates;

public class RequestAdapter(HttpRequest httpRequest, MockedRoute route)
{
    // Request.RouteValues won't work since all requests goes to one endpoint that catches everything
    private Dictionary<string, string>? _routeValues;
    public string GetQueryParam(string key, string defaultValue = "")
    {
        return httpRequest.Query.ContainsKey(key) ? httpRequest.Query[key].ToString() : defaultValue;
    }

    public string GetUrlParam(string key, string defaultValue = "")
    {
        _routeValues ??= UrlParameterExtractor.Extract(route.UrlTemplate, httpRequest.Path);
        return _routeValues.TryGetValue(key, out string? value) ? value?.ToString() ?? defaultValue : defaultValue;
    }

    public string GetCookieParam(string key, string defaultValue = "")
    {
        return httpRequest.Cookies.ContainsKey(key) ? httpRequest.Cookies[key]! : defaultValue;
    }

    public string GetHeaderParam(string key, string defaultValue = "")
    {
        return httpRequest.Headers.TryGetValue(key, out var value) ? value.ToString() : defaultValue;
    }

    public string GetHostname() => httpRequest.Host.Host;

    public string GetIp() => httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public string GetMethod() => httpRequest.Method;

    public string GetPath() => httpRequest.Path;
}
