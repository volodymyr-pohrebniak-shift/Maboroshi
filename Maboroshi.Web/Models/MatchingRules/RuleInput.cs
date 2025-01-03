namespace Maboroshi.Web.Models.MatchingRules;

public class RuleInput : IQueryRuleInput, IHeaderRuleInput, IRouteRuleInput
{
    public required Dictionary<string, string> Headers { get; init; }

    public required Dictionary<string, string> QueryParameters { get; init; }

    public required Dictionary<string, string> RouteParameters { get; init; }
}
