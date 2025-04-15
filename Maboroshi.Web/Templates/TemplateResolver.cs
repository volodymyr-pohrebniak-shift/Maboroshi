using Maboroshi.Web.Models;
using Maboroshi.TemplateEngine;
using Microsoft.Extensions.Caching.Memory;
using Maboroshi.Web.Services;

namespace Maboroshi.Web.Templates;

public class TemplateResolver(IMemoryCache _responseCache, IMemoryCache _templateCache, ICacheResetTokenProvider _tokenProvider)
{
    public string? GetTemplate(RequestAdapter requestAdapter, MockedRouteResponse response)
    {
        if (_responseCache.TryGetValue(requestAdapter.GetPath(), out string? responseStr))
        {
            return responseStr;
        }
        string? compiledResponse;
        var shouldCache = true;
        if (!string.IsNullOrEmpty(response.Body) && !response.DisableTemplating)
        {

            if (!_templateCache.TryGetValue(requestAdapter.GetPath(), out Template? template))
            {
                template = TemplateGenerator.CreateTemplate(response.Body, new(response.StrictTemplateErrors));
            }

            // if we use some params from the request we do not cache template
            void callback() => shouldCache = false;

            compiledResponse = template!.Compile(new RequestParamsFunctionResolver(requestAdapter, callback));

            // we still cache template object, so next time we don't need to parse it again
            if (!shouldCache)
            {
                _templateCache.Set(requestAdapter.GetPath(), template, 
                    new MemoryCacheEntryOptions
                    {
                        ExpirationTokens = { _tokenProvider.GetChangeToken() }
                    });
            }
        }
        else
        {
            compiledResponse = response.Body;
        }
        if (shouldCache)
        {
            _responseCache.Set(requestAdapter.GetPath(), compiledResponse, 
                new MemoryCacheEntryOptions
                {
                    ExpirationTokens = { _tokenProvider.GetChangeToken() }
                });
        }

        return compiledResponse;
    }

    public void ClearCaches()
    {
        (_templateCache as MemoryCache)!.Clear();
        (_responseCache as MemoryCache)!.Clear();
    }
}
