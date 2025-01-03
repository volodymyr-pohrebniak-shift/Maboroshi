using Maboroshi.Web.Models;
using Maboroshi.Web.Models.MatchingRules;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Maboroshi.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IMockedRouteStore>((sp) =>
            {
                var root = new FileConfigurationParser(Path.Combine(builder.Environment.ContentRootPath, "mocks/example.json")).Parse();
                return new InMemoryMockedRouteStore(root!.Environments.First().Routes);
            });

            builder.Services.AddScoped<RequestProcessor>();

            var app = builder.Build();

            app.Map("{**catchAll}", (HttpContext context, RequestProcessor requestProcessor) =>
            {
                return requestProcessor.ProcessRequests(context);
            });

            app.Run();
        }
    }

    public class RequestProcessor(IMockedRouteStore routesStore)
    {
        public async Task<IResult> ProcessRequests(HttpContext context)
        {
            var routes = routesStore.GetRoutesByCriteria(context.Request.Path, MapMethodFromRequest(context.Request.Method));

            if (!routes.Any())
            {
                return Results.NotFound();
            }

            var route = routes.First();
            MockedRouteResponse? selectedResponse = null;

            if (route.ResponseSelectionStrategy == ResponseSelectionStrategy.Default)
            {
                foreach (var response in route.Responses)
                {
                    if (!response.Rules.Any())
                    {
                        selectedResponse = response;
                        break;
                    }

                    if (response.Rules.All(r => r.Evaluate(GetInputFromContext(context.Request, context.Request.Path))))
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

            return new CustomResult(selectedResponse.StatusCode, selectedResponse.Headers.ToDictionary(x => x.Key, x => x.Value), selectedResponse.Body);
        }

        private Models.HttpMethod MapMethodFromRequest(string method) => method switch
        {
                "GET" => Models.HttpMethod.GET,
                "POST" => Models.HttpMethod.POST,
                "PUT" => Models.HttpMethod.PUT,
                "DELETE" => Models.HttpMethod.DELETE,
                "PATCH" => Models.HttpMethod.PATCH,
                _ => Models.HttpMethod.GET
            };

        private static RuleInput GetInputFromContext(HttpRequest request, string path)
        {
            return new RuleInput()
            {
                RouteParameters = new Dictionary<string, string>(),
                QueryParameters = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
                Headers = request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            };
        }
    }

    public interface IMockedRouteStore
    {
        IEnumerable<MockedRoute> GetRoutesByCriteria(string url, Models.HttpMethod method);
    }

    public class InMemoryMockedRouteStore : IMockedRouteStore
    {
        private readonly ConcurrentBag<MockedRoute> _routes;

        public InMemoryMockedRouteStore(IEnumerable<MockedRoute> routes) {
            _routes = new(routes);
        }

        public void AddRoute(MockedRoute route)
        {
            _routes.Add(route);
        }

        public IEnumerable<MockedRoute> GetRoutesByCriteria(string url, Models.HttpMethod method)
        {
            return _routes.Where(route => (route.HttpMethod & method) != 0 && UrlMatchesTemplate(url, route.Url));
        }

        private static bool UrlMatchesTemplate(string url, string template)
        {
            // TODO write proper url to regex converter
            var pattern = "^" + Regex.Escape(template)
                // Replace dynamic segments like :id with named capture groups
                .Replace(@"\:", "(?<")
                // Allow parameters to be separated by /, -, or .
                .Replace(@")", @">[^/.-]+)")
                // Handle optional segments like (cd)?
                .Replace(@"\\\(", "(")
                .Replace(@"\\\)", ")")
                .Replace(@"\\\?", "?")
                // Replace wildcard * with .* to match any sequence of characters
                .Replace(@"\\\*", ".*")
                + "$"; // Ensure full match from start to end

            // Check if the provided URL matches the constructed regex pattern
            return Regex.IsMatch(url, pattern);
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
}
