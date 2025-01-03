namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

[method: JsonConstructor]
public record AggregateRule(IEnumerable<IMatchingRule> rules, AggregateRuleOperation op) : IMatchingRule
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