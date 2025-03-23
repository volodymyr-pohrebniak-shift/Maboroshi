namespace Maboroshi.Web.Models.MatchingRules;

public record RuleInput : IQueryRuleInput, IHeaderRuleInput, IRouteRuleInput
{
    public required Dictionary<string, string> Headers { get; init; }

    public required Dictionary<string, string> QueryParameters { get; init; }

    public required Dictionary<string, string> RouteParameters { get; init; }

    public Dictionary<string, string> CookieParameters { get; init; } = [];
}
