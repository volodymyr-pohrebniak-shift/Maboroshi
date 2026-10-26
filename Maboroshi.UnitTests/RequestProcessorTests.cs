using Maboroshi.Web;
using Maboroshi.Web.Models;
using Maboroshi.Web.Models.MatchingRules;
using Maboroshi.Web.RouteMatching;
using Maboroshi.Web.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using HttpMethod = Maboroshi.Web.Models.HttpMethod;

namespace Maboroshi.UnitTests;

public class RequestProcessorTests
{
    [Fact]
    public async Task Process_ShouldRenderTheBody_OfTheSelectedResponse_OnEveryRequest()
    {
        var processor = CreateProcessor(
            Response("authorized", 200, "{{ uppercase 'ok' }}", new HeaderMatchingRule("X-Auth", "1", MatchingRuleOperation.Equals, false)),
            Response("denied", 401, "{{ uppercase 'denied' }}"));

        await AssertResponse(processor, auth: false, 401, "DENIED");
        await AssertResponse(processor, auth: true, 200, "OK");
        await AssertResponse(processor, auth: false, 401, "DENIED");
    }

    [Fact]
    public async Task Process_ShouldGenerateFakeData_OnEveryRequest()
    {
        var processor = CreateProcessor(Response("guid", 200, "{{ guid }}"));

        var first = Assert.IsType<CustomResult>(await processor.Process(CreateContext())).Content;
        var second = Assert.IsType<CustomResult>(await processor.Process(CreateContext())).Content;

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task Process_ShouldReturnTheReason_WhenStrictTemplateFails()
    {
        var processor = CreateProcessor(Response("broken", 200, "{{ nope }}", strict: true));

        var problem = Assert.IsType<ProblemHttpResult>(await processor.Process(CreateContext()));

        Assert.Equal(500, problem.StatusCode);
        Assert.Contains("broken", problem.ProblemDetails.Title);
        Assert.Contains("nope is not defined", problem.ProblemDetails.Detail);
    }

    private static async Task AssertResponse(RequestProcessor processor, bool auth, int statusCode, string body)
    {
        var context = CreateContext();
        if (auth)
            context.Request.Headers["X-Auth"] = "1";

        var result = Assert.IsType<CustomResult>(await processor.Process(context));

        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(body, result.Content);
    }

    private static RequestProcessor CreateProcessor(params MockedRouteResponse[] responses)
    {
        var store = new InMemoryMockedRouteStore(new MatchEverything());
        store.SetRoutes([new MockedRoute("/api/items", HttpMethod.GET, responses, ResponseSelectionStrategy.Default, true)]);
        return new RequestProcessor(store, new TemplateResolver());
    }

    private static MockedRouteResponse Response(string name, int statusCode, string body, IMatchingRule? rule = null, bool strict = false) => new()
    {
        Name = name,
        StatusCode = statusCode,
        Body = body,
        Rules = rule is null ? [] : [rule],
        StrictTemplateErrors = strict
    };

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/items";
        return context;
    }

    private class MatchEverything : IUrlMatchingHandler
    {
        public bool MatchesRoute(string template, string url) => true;
    }
}
