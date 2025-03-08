using FluentAssertions;
using Maboroshi.Web;
using Maboroshi.Web.Models;
using Maboroshi.Web.RouteMatching;
using Microsoft.AspNetCore.Routing;

namespace Maboroshi.UnitTests;

public class UrlMatchingTests
{
    [Theory]
    // Basic static routes
    [InlineData("home", "home", true)]
    [InlineData("about", "about", true)]
    [InlineData("home", "home/extra", false)]
    [InlineData("about", "notabout", false)]

    // Dynamic parameterized routes
    [InlineData("products/{id}", "products/123", true)]
    [InlineData("users/{username}", "users/john", true)]
    [InlineData("users/{username}", "users/", false)]
    [InlineData("products/{id}", "products", false)]

    // Routes with constraints
    [InlineData("orders/{id:int}", "orders/100", true)]
    [InlineData("orders/{id:int}", "orders/abc", false)]
    [InlineData("files/{fileName:alpha}", "files/report", true)]
    [InlineData("files/{fileName:alpha}", "files/1234", false)]

    // Multiple parameters
    [InlineData("items/{category}/{itemId:int}", "items/electronics/500", true)]
    [InlineData("items/{category}/{itemId:int}", "items/electronics/notAnInt", false)]

    // Optional parameters
    [InlineData("blog/{article?}", "blog/", true)]
    [InlineData("blog/{article?}", "blog/how-to-code", true)]
    [InlineData("profile/{username?}/settings", "profile/settings", false)]
    [InlineData("profile/{username?}/settings", "profile/john/settings", true)]

    // Wildcard routes
    [InlineData("search/{*query}", "search/apple iphone", true)]
    [InlineData("search/{*query}", "search/", true)]
    [InlineData("assets/{**path}", "assets/images/background.jpg", true)]
    [InlineData("assets/{**path}", "assets/docs/userguide.pdf", true)]

    // Default values
    [InlineData("dashboard/{section=overview}", "dashboard/", true)]
    [InlineData("dashboard/{section=overview}", "dashboard/stats", true)]
    [InlineData("dashboard/{section=overview}", "dashboard/stats/details", false)]

    // Edge cases
    [InlineData("products/{id:int}", "products/-5", true)] // Negative number should match
    [InlineData("users/{username}", "users/JohnDoe123", true)] // Alphanumeric username

    public void Route_ShouldMatch_Correctly(string template, string url, bool expectedMatch)
    {
        var routeStrategy = new AspNetCoreUrlMatchingStrategy(template);
        var isMatch = routeStrategy.IsUrlMatch(url);
        isMatch.Should().Be(expectedMatch, $"because '{url}' should {(expectedMatch ? "" : "not ")}match '{template}'");
    }
}
