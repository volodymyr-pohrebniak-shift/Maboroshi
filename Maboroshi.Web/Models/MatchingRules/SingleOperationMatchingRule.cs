namespace Maboroshi.Web.Models.MatchingRules;

using Maboroshi.Web.Utils;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public abstract class SingleOperationMatchingRule(string? key, string? value, MatchingRuleOperation operation, bool negate) : BaseMatchingRule
{
    public string Key { get; } = Guard.Against.NullOrWhiteSpace(key, nameof(key));
    public string Value { get; } = value ?? string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MatchingRuleOperation Operation { get; } = operation;
    public bool Negate { get; } = negate;

    protected bool ApplyOperation(string? input)
    {
        return Operation switch
        {
            MatchingRuleOperation.Equals => Negate ^ Value.Equals(input),
            MatchingRuleOperation.Contains => Negate ^ input!.Contains(Value),
            MatchingRuleOperation.NullOrEmpty => Negate ^ string.IsNullOrEmpty(input),
            MatchingRuleOperation.Regex => Negate ^ new Regex(Value).IsMatch(input!),
            _ => throw new NotImplementedException($"Operation {Operation} not implemented.")
        };
    }
}

public enum MatchingRuleOperation
{
    Equals,
    Contains,
    Regex,
    NullOrEmpty
}
