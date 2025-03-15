using Maboroshi.Web.Models;
using Maboroshi.Web.Models.MatchingRules;
using Maboroshi.Web.Templates;

namespace Maboroshi.Web;

public class RequestProcessor(IMockedRouteStore routesStore, TemplateResolver templateResolver)
{
    public IResult Process(HttpContext context)
    {
        var route = routesStore.GetRouteByCriteria(context.Request.Path, MapMethodFromRequest(context.Request.Method));

        if (route is null)
        {
            return Results.NotFound();
        }

        MockedRouteResponse? selectedResponse = null;

        var requestData = GetInputFromRequest(context.Request, route.UrlTemplate);
        if (route.ResponseSelectionStrategy == ResponseSelectionStrategy.Default)
        {
            foreach (var response in route.Responses)
            {
                if (!response.Rules.Any())
                {
                    selectedResponse = response;
                    break;
                }

                if (response.Rules.All(r => r.Evaluate(requestData)))
                {
                    selectedResponse = response;
                    break;
                }
            }
        }

        if (selectedResponse is null)
        {
            return Results.NotFound();
        }

        foreach (var header in selectedResponse.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        var compiledResponse = templateResolver.GetTemplate(new RequestAdapter(context.Request, route), selectedResponse);

        return new CustomResult(selectedResponse.StatusCode, selectedResponse.Headers.ToDictionary(x => x.Key, x => x.Value), compiledResponse);
    }

    private static Models.HttpMethod MapMethodFromRequest(string method) => method switch
    {
            "GET" => Models.HttpMethod.GET,
            "POST" => Models.HttpMethod.POST,
            "PUT" => Models.HttpMethod.PUT,
            "DELETE" => Models.HttpMethod.DELETE,
            "PATCH" => Models.HttpMethod.PATCH,
            _ => Models.HttpMethod.GET
        };

    private static RuleInput GetInputFromRequest(HttpRequest request, string urlTemplate)
    {
        return new RuleInput()
        {
            RouteParameters = UrlParameterExtractor.Extract(urlTemplate, request.Path),
            QueryParameters = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
        };
    }
}
