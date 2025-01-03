using Maboroshi.Web.Models.MatchingRules;

namespace Maboroshi.UnitTests.MatchingRules;

public class AggregateMatchingRuleTests
{
    [Fact]
    public void AggregateRule_ShouldMatch_WhenAllRulesMatch_WithAND()
    {
        var rules = new List<IMatchingRule>
        {
            new HeaderMatchingRule("Authorization", "Bearer token123", MatchingRuleOperation.Equals, false),
            new QueryMatchingRule("filter", "test", MatchingRuleOperation.Contains, false)
        };

        var aggregateRule = new AggregateRule(rules, AggregateRuleOperation.AND);

        var input = new RuleInput()
        {
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" }
            },
            QueryParameters = new Dictionary<string, string>
            {
                { "filter", "test" }
            },
            RouteParameters = []
        };

        Assert.True(aggregateRule.Evaluate(input));
    }

    [Fact]
    public void AggregateRule_ShouldNotMatch_WhenAllRulesMatch_WithAND_AndInputIsWrong()
    {
        var rules = new List<IMatchingRule>
        {
            new HeaderMatchingRule("Authorization", "Bearer token123", MatchingRuleOperation.Equals, false),
            new QueryMatchingRule("filter", "test", MatchingRuleOperation.Contains, false)
        };

        var aggregateRule = new AggregateRule(rules, AggregateRuleOperation.AND);

        var input = new RuleInput()
        {
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Invalid" }
            },
            QueryParameters = new Dictionary<string, string>
            {
                { "filter", "qwerty" }
            },
            RouteParameters = []
        };

        Assert.False(aggregateRule.Evaluate(input));
    }

    [Fact]
    public void AggregateRule_ShouldMatch_WhenOneRuleMatches_WithOR()
    {
        var rules = new List<IMatchingRule>
        {
            new HeaderMatchingRule("Authorization", "Bearer token123", MatchingRuleOperation.Equals, false),
            new QueryMatchingRule("filter", "test", MatchingRuleOperation.Contains, false)
        };

        var aggregateRule = new AggregateRule(rules, AggregateRuleOperation.OR);

        var input = new RuleInput()
        {
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer token123" }
            },
            QueryParameters = new Dictionary<string, string>
            {
                { "filter", "qwerty" }
            },
            RouteParameters = []
        };

        Assert.True(aggregateRule.Evaluate(input));
    }
}
