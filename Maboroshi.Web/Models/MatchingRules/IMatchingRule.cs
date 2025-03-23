using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models.MatchingRules;

public interface IMatchingRule
{
    bool Evaluate(IRuleInput input);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HeaderMatchingRule), typeDiscriminator: nameof(MatchingRuleType.Header))]
[JsonDerivedType(typeof(QueryMatchingRule), typeDiscriminator: nameof(MatchingRuleType.Query))]
[JsonDerivedType(typeof(RouteMatchingRule), typeDiscriminator: nameof(MatchingRuleType.Route))]
public abstract class BaseMatchingRule : IMatchingRule
{
    public abstract bool Evaluate(IRuleInput input);
}

public interface IRuleInput
{
}
