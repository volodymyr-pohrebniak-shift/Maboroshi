using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Maboroshi.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.Map("{**catchAll}", (HttpContext context) =>
            {
                return Results.Content("This is the catch-all handler. Path: " + context.Request.Path);
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


            // set response
            await Task.Delay(0);

            return Results.NotFound();
        }

        private HttpMethod MapMethodFromRequest(string method) => method switch
        {
                "GET" => HttpMethod.GET,
                "POST" => HttpMethod.POST,
                "PUT" => HttpMethod.PUT,
                "DELETE" => HttpMethod.DELETE,
                "PATCH" => HttpMethod.PATCH,
                _ => HttpMethod.GET
            };
    }

    public interface IMockedRouteStore
    {
        void AddRoute(MockedRoute route);
        IEnumerable<MockedRoute> GetRoutesByCriteria(string url, HttpMethod method);
    }

    public class MockedRouteStore
    {
        private readonly ConcurrentBag<MockedRoute> _routes = [];

        public void AddRoute(MockedRoute route)
        {
            _routes.Add(route);
        }

        public IEnumerable<MockedRoute> GetRoutesByCriteria(string url, HttpMethod method)
        {
            return _routes.Where(route => (route.HttpMethod & method) != 0 && UrlMatchesTemplate(url, route.Url));
        }

        private bool UrlMatchesTemplate(string url, string template)
        {
            // Escape special characters to treat them as literals
            string pattern = "^" + Regex.Escape(template)
                // Replace dynamic segments like :id with named capture groups
                .Replace(@"\:", "(?<")
                // Keep slashes as-is for path separation
                .Replace(@"/", "/")
                // Close the capture group and match one or more non-slash characters
                .Replace(@")", @">[^/]+)")
                // Replace wildcard * with .* to match any sequence of characters
                .Replace(@"\\\*", ".*")  // Support wildcard (*) in paths
                + "$"; // Ensure full match from start to end

            // Check if the provided URL matches the constructed regex pattern
            return Regex.IsMatch(url, pattern);
        }
    }

    [Flags]
    public enum HttpMethod
    {
        NONE = 0,
        GET = 1,
        POST = 2,
        PUT = 4,
        DELETE = 8,
        PATCH = 16
    }

    public class MockedRoute
    {
        public required string Url { get; set; }
        public HttpMethod HttpMethod { get; set; }
        public IEnumerable<Response> Responses { get; }
        public ResponseSelectionStrategy ResponseSelectionStrategy { get; }
    }

    public enum ResponseSelectionStrategy
    {
        Default,
        Random,
        Sequence
    }

    public class Response
    {
        public string StatusCode { get; }
        public string Content { get; }
        public IEnumerable<IMatchingRule> Rules { get; }
        public IEnumerable<Header> Headers { get; }
    }

    public class Header
    {
        public string Key { get; }
        public string Value { get; }
    }

    public interface IRuleInput
    {
        Dictionary<string, string> RequestHeaders { get; }
        Dictionary<string, string> QueryParameters { get; }
        Dictionary<string, string> RouteParameters { get; }
    }

    public interface IMatchingRule
    {
        bool Evaluate(IRuleInput input);
    }

    public class SingleOperationMatchingRule(MatchingRuleType ruleType, string? key, string? value, MatchingRuleOperation operation, bool negate) : IMatchingRule
    {
        public bool Evaluate(IRuleInput input)
        {
            switch (ruleType)
            {
                case MatchingRuleType.Header:
                    if (input.RequestHeaders.TryGetValue(key!, out var headerValue))
                    {
                        switch (operation)
                        {
                            case MatchingRuleOperation.Equals:
                                return !negate && headerValue.Equals(value);
                            case MatchingRuleOperation.Contains:
                                return !negate && headerValue.Contains(value!);
                            case MatchingRuleOperation.NullOrEmpty:
                                return !negate && string.IsNullOrEmpty(headerValue);
                            case MatchingRuleOperation.Regex:
                                return !negate && new Regex(value!).IsMatch(headerValue);
                        }
                    } else
                    {
                        return false;
                    }
                    break;
                case MatchingRuleType.Route:
                    if (input.RouteParameters.TryGetValue(key!, out var routeValue))
                    {
                        switch (operation)
                        {
                            case MatchingRuleOperation.Equals:
                                return !negate && routeValue.Equals(value);
                            case MatchingRuleOperation.Contains:
                                return !negate && routeValue.Contains(value!);
                            case MatchingRuleOperation.NullOrEmpty:
                                return !negate && string.IsNullOrEmpty(routeValue);
                            case MatchingRuleOperation.Regex:
                                return !negate && new Regex(value!).IsMatch(routeValue);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case MatchingRuleType.Query:
                    if (input.RouteParameters.TryGetValue(key!, out var queryValue))
                    {
                        switch (operation)
                        {
                            case MatchingRuleOperation.Equals:
                                return !negate && queryValue.Equals(value);
                            case MatchingRuleOperation.Contains:
                                return !negate && queryValue.Contains(value!);
                            case MatchingRuleOperation.NullOrEmpty:
                                return !negate && string.IsNullOrEmpty(queryValue);
                            case MatchingRuleOperation.Regex:
                                return !negate && new Regex(value!).IsMatch(queryValue);
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }


            throw new NotImplementedException();
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

    public class AggregateRule(IEnumerable<IMatchingRule> rules, AggregateRuleOperation op) : IMatchingRule
    {
        public bool Evaluate(IRuleInput input)
        {
            if (!rules.Any())
                return false;

            foreach (var rule in rules)
            {
                var result = rule.Evaluate(input);

                if (op == AggregateRuleOperation.AND && !result)
                    return false;
                if (op == AggregateRuleOperation.OR && result)
                    return true;
            }

            return op == AggregateRuleOperation.AND;
        }
    }

    public enum AggregateRuleOperation
    {
        AND,
        OR
    }

    public enum MatchingRuleOperation
    {
        Equals,
        Contains,
        Regex,
        NullOrEmpty
    }
}
