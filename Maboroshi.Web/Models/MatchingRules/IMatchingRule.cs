namespace Maboroshi.Web.Models.MatchingRules;

public interface IMatchingRule
{
    bool Evaluate(IRuleInput input);
}

public interface IRuleInput
{
}
