using Maboroshi.Web.Models;
using Maboroshi.Web.Models.MatchingRules;
using Maboroshi.Web.RouteMatching;
using Maboroshi.TemplateEngine;

namespace Maboroshi.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IMockedRouteStore>((sp) =>
        {
            var root = new FileConfigurationParser(Path.Combine(builder.Environment.ContentRootPath, "mocks/example.json")).Parse();
            return new InMemoryMockedRouteStore(root!.Environments.First().Routes, new UrlMatchingHandler(new UrlMatchinStrategyFactory()));
        });

        builder.Services.AddScoped<RequestProcessor>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapGet("/$$$SYSTEM$$$/environments", (IMockedRouteStore routesStore) =>
        {
            return routesStore.GetAll();
        });

        app.Map("{**catchAll}", (HttpContext context, RequestProcessor requestProcessor) => requestProcessor.ProcessRequests(context));

        //app.MapFallbackToFile("/index.html");

        app.Run();
    }
}

public class RequestProcessor(IMockedRouteStore routesStore)
{
    public async Task<IResult> ProcessRequests(HttpContext context)
    {
        var route = routesStore.GetRouteByCriteria(context.Request.Path, MapMethodFromRequest(context.Request.Method));

        if (route is null)
        {
            return Results.NotFound();
        }
        
        MockedRouteResponse? selectedResponse = null;

        var requestData = GetInputFromRequest(context.Request, route.UrlTemplate, context.Request.Path);
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
        
        var compiledResponse = !string.IsNullOrEmpty(selectedResponse.Body) 
            ? GetResponseBody(context.Request.Path, selectedResponse.Body!) 
            : selectedResponse.Body;

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

    private static RuleInput GetInputFromRequest(HttpRequest request, string urlTemplate, string path)
    {
        return new RuleInput()
        {
            RouteParameters = UrlParameterExtractor.Extract(urlTemplate, path),
            QueryParameters = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
        };
    }

    private string GetResponseBody(string url, string templateString)
    {
        var templateEngine = new TemplateGenerator();
        
        var template = templateEngine.CreateTemplate(templateString);

        return template.Compile();
    }
}

public enum MatchingRuleType
{
    Header,
    Route,
    Query,
    Cookie,
    Body
}
