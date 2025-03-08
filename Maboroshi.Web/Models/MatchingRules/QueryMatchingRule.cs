namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

public interface IQueryRuleInput : IRuleInput
{
    Dictionary<string, string> QueryParameters { get; }
}

[method: JsonConstructor]
public class QueryMatchingRule(string? key, string? value, MatchingRuleOperation operation, bool negate) : SingleOperationMatchingRule(key, value, operation, negate)
{
    public override bool Evaluate(IRuleInput input)
    {
        if (input is not IQueryRuleInput queryRuleInput)
            throw new ArgumentException($"Invalid input type for {nameof(HeaderMatchingRule)}");

        return ApplyOperation(queryRuleInput.QueryParameters.GetValueOrDefault(Key, string.Empty));
    }
}
