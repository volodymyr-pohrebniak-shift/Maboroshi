namespace Maboroshi.UnitTests.MatchingRules;

using Maboroshi.Web.Models.MatchingRules;

public class SingleOperationMatchingRuleTests
{
    [Theory]
    [InlineData("Authorization", "Bearer token123", "Bearer token123", false, true)]
    [InlineData("Authorization", "Bearer token123", "Invalid", false, false)]
    [InlineData("Authorization", "Bearer token123", "Bearer token123", true, false)]
    [InlineData("Authorization", "Bearer token123", "Invalid", true, true)]
    public void MatchingRule_ShouldEvaluate_Header(string key, string value, string inputHeader, bool negate, bool expectedResult)
    {
        var rule = new HeaderMatchingRule(key, value, MatchingRuleOperation.Equals, negate);

        var input = new RuleInput
        {
            Headers = new Dictionary<string, string> { { key, inputHeader } },
            QueryParameters = [],
            RouteParameters = []
        };

        Assert.Equal(expectedResult, rule.Evaluate(input));
    }

    [Theory]
    [InlineData("id", "42", "42", false, true)]
    [InlineData("id", "42", "43", false, false)]
    [InlineData("id", "42", "42", true, false)]
    [InlineData("id", "42", "43", true, true)]
    public void MatchingRule_ShouldEvaluate_Route(string key, string value, string routeParam, bool negate, bool expectedResult)
    {
        var rule = new RouteMatchingRule(key, value, MatchingRuleOperation.Equals, negate);

        var input = new RuleInput
        {
            Headers = [],
            QueryParameters = [],
            RouteParameters = new Dictionary<string, string> { { key, routeParam } }
        };

        Assert.Equal(expectedResult, rule.Evaluate(input));
    }

    [Theory]
    [InlineData("query", "search", "search", false, true)]
    [InlineData("query", "search", "different", false, false)]
    [InlineData("query", "search", "search", true, false)]
    [InlineData("query", "search", "different", true, true)]
    public void MatchingRule_ShouldEvaluate_Query(string key, string value, string queryParam, bool negate, bool expectedResult)
    {
        var rule = new QueryMatchingRule(key, value, MatchingRuleOperation.Equals, negate);

        var input = new RuleInput
        {
            Headers = [],
            QueryParameters = new Dictionary<string, string> { { key, queryParam } },
            RouteParameters = []
        };

        Assert.Equal(expectedResult, rule.Evaluate(input));
    }

    [Theory]
    [InlineData(typeof(HeaderMatchingRule))]
    [InlineData(typeof(RouteMatchingRule))]
    [InlineData(typeof(QueryMatchingRule))]
    public void Rule_ShouldThrowArgumentNullException_WhenKeyIsEmpty(Type ruleType)
    {
        Assert.Throws<ArgumentNullException>(() =>  CreateRule(ruleType, "", "test", MatchingRuleOperation.Equals, true));
    }

    [Theory]
    [InlineData(typeof(HeaderMatchingRule))]
    [InlineData(typeof(RouteMatchingRule))]
    [InlineData(typeof(QueryMatchingRule))]
    public void Rule_ShouldThrowArgumentException_WhenInputTypeIsInvalid(Type ruleType)
    {
        var rule = CreateRule(ruleType, "test", "test", MatchingRuleOperation.Equals, true);
        var invalidInput = new FakeRuleInput();

        Assert.Throws<ArgumentException>(() => rule.Evaluate(invalidInput));
    }

    private IMatchingRule CreateRule(Type ruleType, string key, string value, MatchingRuleOperation operation, bool negate)
    {
        return ruleType switch
        {
            _ when ruleType == typeof(HeaderMatchingRule) => new HeaderMatchingRule(key, value, operation, negate),
            _ when ruleType == typeof(RouteMatchingRule) => new RouteMatchingRule(key, value, operation, negate),
            _ when ruleType == typeof(QueryMatchingRule) => new QueryMatchingRule(key, value, operation, negate),
            _ => throw new NotImplementedException($"Unknown rule type: {ruleType}")
        };
    }

    private IRuleInput CreateInput(Type ruleType, string key, string input)
    {
        return ruleType switch
        {
            _ when ruleType == typeof(HeaderMatchingRule) =>
                new RuleInput { Headers = new Dictionary<string, string> { { key, input } }, QueryParameters = [], RouteParameters = [] },
            _ when ruleType == typeof(RouteMatchingRule) =>
                new RuleInput { RouteParameters = new Dictionary<string, string> { { key, input } }, Headers = [], QueryParameters = [] },
            _ when ruleType == typeof(QueryMatchingRule) =>
                new RuleInput { QueryParameters = new Dictionary<string, string> { { key, input } }, Headers = [], RouteParameters = [] },
            _ => throw new NotImplementedException($"Unknown input type: {ruleType}")
        };
    }

    class FakeRuleInput : IRuleInput { }
}
