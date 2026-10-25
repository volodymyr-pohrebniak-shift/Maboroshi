namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

[method: JsonConstructor]
public class AggregateRule(IEnumerable<IMatchingRule> rules, AggregateRuleOperation op) : BaseMatchingRule
{
    public IEnumerable<IMatchingRule> Rules { get; } = rules?.Any() == true
        ? rules
        : throw new ArgumentException("A group of rules can't be empty", nameof(rules));

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AggregateRuleOperation Op { get; } = op;

    public override bool Evaluate(IRuleInput input)
    {
        foreach (var rule in Rules)
        {
            var result = rule.Evaluate(input);

            switch (Op)
            {
                case AggregateRuleOperation.AND when !result:
                    return false;
                case AggregateRuleOperation.OR when result:
                    return true;
            }
        }

        return Op == AggregateRuleOperation.AND;
    }
}

public enum AggregateRuleOperation
{
    AND,
    OR
}