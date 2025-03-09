using Maboroshi.Web.Models;
using Maboroshi.TemplateEngine;
using Microsoft.Extensions.Caching.Memory;

namespace Maboroshi.Web.Templates;

public class TemplateResolver(IMemoryCache _cache)
{
    public string? GetTemplate(RequestAdapter requestAdapter, MockedRouteResponse response)
    {
        if (_cache.TryGetValue(requestAdapter.GetPath(), out var template))
        {
            return (string)template!;
        }
        string? compiledResponse;
        bool shouldCache = true;
        if (!string.IsNullOrEmpty(response.Body))
        {
            var newTemplate = TemplateGenerator.CreateTemplate(response.Body);

            // if we use some params from the request we do not cache template
            void callback() => shouldCache = false;

            compiledResponse = newTemplate.Compile(new RequestParamsFunctionResolver(requestAdapter, callback));
        }
        else
        {
            compiledResponse = response.Body;
        }
        if (shouldCache && response.AllowResponseCaching)
        {

            _cache.Set(requestAdapter.GetPath(), compiledResponse);
        }

        return compiledResponse;
    }
}
