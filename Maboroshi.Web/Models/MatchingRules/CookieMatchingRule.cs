namespace Maboroshi.Web.Models.MatchingRules;

using System.Text.Json.Serialization;

public interface ICookieRuleInput : IRuleInput
{
    Dictionary<string, string> CookieParameters { get; }
}

[method: JsonConstructor]
public class CookieMatchingRule(string? key, string? value, MatchingRuleOperation operation, bool negate) : SingleOperationMatchingRule(key, value, operation, negate)
{
    public override bool Evaluate(IRuleInput input)
    {
        if (input is not ICookieRuleInput cookieRuleInput)
            throw new ArgumentException($"Invalid input type for {nameof(CookieMatchingRule)}");

        return ApplyOperation(cookieRuleInput.CookieParameters.GetValueOrDefault(Key, string.Empty));
    }
}
