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

            switch (op)
            {
                case AggregateRuleOperation.AND when !result:
                    return false;
                case AggregateRuleOperation.OR when result:
                    return true;
            }
        }

        return op == AggregateRuleOperation.AND;
    }
}

public enum AggregateRuleOperation
{
    AND,
    OR
}