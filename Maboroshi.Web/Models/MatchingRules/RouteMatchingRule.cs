namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

public interface IRouteRuleInput : IRuleInput
{
    Dictionary<string, string> RouteParameters { get; }
}

[method: JsonConstructor]
public class RouteMatchingRule(string? key, string? value, MatchingRuleOperation operation, bool negate) : SingleOperationMatchingRule(key, value, operation, negate)
{
    public override bool Evaluate(IRuleInput input)
    {
        if (input is not IRouteRuleInput routeRuleInput)
            throw new ArgumentException($"Invalid input type for {nameof(HeaderMatchingRule)}");

        return routeRuleInput.RouteParameters.TryGetValue(Key, out var routeValue) && ApplyOperation(routeValue);
    }
}
