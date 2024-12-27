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

    public class RequestProcessor
    {
        public Task ProcessRequests(HttpContext context)
        {
            // find route by path method

            // validate rules if any

            // set response

            return Task.CompletedTask;
        }
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



    }

    public interface IRuleInput
    {

    }

    public interface IMatchingRule
    {
        bool Evaluate(IRuleInput input);
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

    public enum MatchingRuleOPeration
    {
        Equals,
        Contains,
        Regex,
        NullOrEmpty
    }
}
