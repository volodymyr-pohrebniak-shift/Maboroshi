using System.Text.Json;
using Maboroshi.Web;
using Maboroshi.Web.Converters;
using Maboroshi.Web.Models.MatchingRules;
using Environment = Maboroshi.Web.Models.Environment;
using HttpMethod = Maboroshi.Web.Models.HttpMethod;

namespace Maboroshi.UnitTests;

// what the React client sends and expects from /$$$SYSTEM$$$/environments
public class JsonContractTests
{
    // the same options the API uses: web defaults plus the rule converter
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonMatchingRuleConverter() }
    };

    // shaped like the client sends it: client side ids, "type" not the first property, methods as an array
    private const string ClientJson = """
        [{
          "id": "e1", "name": "default", "active": true,
          "routes": [{
            "id": "r1", "urlTemplate": "/api/users/{id}", "httpMethod": ["GET", "POST"],
            "responseSelectionStrategy": "Default", "enabled": true,
            "responses": [{
              "id": "p1", "name": "Response 1", "statusCode": "200", "body": "{}", "delay": 0,
              "headers": [{ "id": "h1", "key": "Content-Type", "value": "application/json" }],
              "rules": [
                { "id": "1", "type": "Header", "key": "Authorization", "operation": "StartsWith", "value": "Bearer", "negate": false },
                { "type": "Aggregate", "id": "2", "op": "OR", "rules": [
                  { "id": "3", "type": "Cookie", "key": "session", "operation": "Equals", "value": "abc", "negate": false },
                  { "id": "4", "type": "Query", "key": "debug", "operation": "NullOrEmpty", "value": "", "negate": true }
                ]}
              ]
            }]
          }]
        }]
        """;

    [Fact]
    public void Deserialize_ShouldReadConfiguration_AsClientSendsIt()
    {
        var environment = Assert.Single(JsonSerializer.Deserialize<Environment[]>(ClientJson, Options)!);

        Assert.True(environment.IsActive);
        var route = Assert.Single(environment.Routes);
        Assert.Equal(HttpMethod.GET | HttpMethod.POST, route.HttpMethod);

        var rules = Assert.Single(route.Responses).Rules.ToList();
        Assert.Equal(MatchingRuleOperation.StartsWith, Assert.IsType<HeaderMatchingRule>(rules[0]).Operation);
        var group = Assert.IsType<AggregateRule>(rules[1]);
        Assert.Equal(AggregateRuleOperation.OR, group.Op);
        Assert.IsType<CookieMatchingRule>(group.Rules.First());
        Assert.IsType<QueryMatchingRule>(group.Rules.Last());
    }

    [Fact]
    public void Serialize_ShouldWriteConfiguration_AsClientReadsIt()
    {
        var environments = JsonSerializer.Deserialize<Environment[]>(ClientJson, Options)!;

        var json = JsonSerializer.SerializeToElement(environments, Options)[0];

        Assert.True(json.GetProperty("active").GetBoolean());
        var route = json.GetProperty("routes")[0];
        Assert.Equal(["GET", "POST"], route.GetProperty("httpMethod").EnumerateArray().Select(m => m.GetString()));
        var rules = route.GetProperty("responses")[0].GetProperty("rules");
        Assert.Equal("Header", rules[0].GetProperty("type").GetString());
        Assert.Equal("StartsWith", rules[0].GetProperty("operation").GetString());
        Assert.Equal("Aggregate", rules[1].GetProperty("type").GetString());
        Assert.Equal("OR", rules[1].GetProperty("op").GetString());
        Assert.Equal("Cookie", rules[1].GetProperty("rules")[0].GetProperty("type").GetString());
    }

    [Fact]
    public void Serialize_ShouldProduceJson_ThatCanBeReadBack()
    {
        var environments = JsonSerializer.Deserialize<Environment[]>(ClientJson, Options)!;

        var roundTripped = JsonSerializer.Deserialize<Environment[]>(JsonSerializer.Serialize(environments, Options), Options)!;

        var group = Assert.IsType<AggregateRule>(roundTripped[0].Routes.First().Responses.First().Rules.Last());
        Assert.Equal(2, group.Rules.Count());
    }

    [Theory]
    [InlineData(HttpMethod.NONE, "[]")]
    [InlineData(HttpMethod.GET, "[\"GET\"]")]
    [InlineData(HttpMethod.PUT | HttpMethod.PATCH, "[\"PUT\",\"PATCH\"]")]
    public void HttpMethod_ShouldBeWrittenAsArray(HttpMethod method, string expected)
    {
        Assert.Equal(expected, JsonSerializer.Serialize(method, new JsonSerializerOptions { Converters = { new JsonHttpMethodConverter() } }));
    }

    [Theory]
    [InlineData("""{ "key": "a", "type": "Body" }""", "Rule type 'Body' is not supported.")]
    [InlineData("""{ "key": "a" }""", "Rule type '' is not supported.")]
    [InlineData("""{ "key": "", "type": "Header" }""", "Invalid Header rule: ")]
    [InlineData("""{ "type": "Aggregate", "op": "AND", "rules": [] }""", "Invalid Aggregate rule: A group of rules can't be empty")]
    public void Deserialize_ShouldExplainInvalidRule(string ruleJson, string expectedMessage)
    {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<IMatchingRule>(ruleJson, Options));

        Assert.StartsWith(expectedMessage, exception.Message);
    }

    [Fact]
    public void FileConfiguration_ShouldBeReadWithTheSameModel()
    {
        var root = new FileConfigurationParser(Path.Combine(AppContext.BaseDirectory, "mocks", "example.json")).Parse()!;

        var environment = Assert.Single(root.Environments);
        Assert.True(environment.IsActive);
        // the file keeps the comma separated form
        Assert.Contains(environment.Routes, r => r.HttpMethod == (HttpMethod.GET | HttpMethod.POST));
        Assert.Contains(environment.Routes.SelectMany(r => r.Responses).SelectMany(r => r.Rules), r => r is HeaderMatchingRule);
    }
}
