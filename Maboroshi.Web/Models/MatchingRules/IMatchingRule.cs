namespace Maboroshi.Web.Models.MatchingRules;

// serialized by JsonMatchingRuleConverter, which maps the "type" property to a rule class
public interface IMatchingRule
{
    bool Evaluate(IRuleInput input);
}

public abstract class BaseMatchingRule : IMatchingRule
{
    public abstract bool Evaluate(IRuleInput input);
}

public interface IRuleInput
{
}
