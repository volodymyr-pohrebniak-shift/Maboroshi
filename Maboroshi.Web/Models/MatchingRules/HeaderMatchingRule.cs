namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

public interface IHeaderRuleInput : IRuleInput
{
    public Dictionary<string, string> Headers { get; }
}

[method: JsonConstructor]
public class HeaderMatchingRule(string? key, string? value, MatchingRuleOperation operation, bool negate) : SingleOperationMatchingRule(key, value, operation, negate)
{
    public override bool Evaluate(IRuleInput input)
    {
        if (input is not IHeaderRuleInput headerRuleInput)
            throw new ArgumentException($"Invalid input type for {nameof(HeaderMatchingRule)}");

        return ApplyOperation(headerRuleInput.Headers.GetValueOrDefault(Key, string.Empty));
    }
}
