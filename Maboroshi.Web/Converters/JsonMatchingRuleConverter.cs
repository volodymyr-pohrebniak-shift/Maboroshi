namespace Maboroshi.Web.Converters;

using Maboroshi.Web.Models.MatchingRules;
using System.Text.Json.Serialization;
using System.Text.Json;

// Picks the rule class by its "type" property wherever it's placed in the object:
// built-in polymorphism in .NET 8 requires "type" to come first, which clients don't guarantee
public class JsonMatchingRuleConverter : JsonConverter<IMatchingRule>
{
    private static readonly Dictionary<string, Type> RuleTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Header"] = typeof(HeaderMatchingRule),
        ["Query"] = typeof(QueryMatchingRule),
        ["Route"] = typeof(RouteMatchingRule),
        ["Cookie"] = typeof(CookieMatchingRule),
        ["Aggregate"] = typeof(AggregateRule),
    };

    public override IMatchingRule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;

        string? typeName = null;
        if (jsonObject.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonObject.EnumerateObject())
            {
                if (property.Name.Equals("type", StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.String)
                    typeName = property.Value.GetString();
            }
        }

        if (typeName is null || !RuleTypes.TryGetValue(typeName, out var ruleType))
            throw new JsonException($"Rule type '{typeName}' is not supported.");

        try
        {
            return (IMatchingRule)jsonObject.Deserialize(ruleType, options)!;
        }
        // rule constructors validate their arguments
        catch (ArgumentException e)
        {
            throw new JsonException($"Invalid {typeName} rule: {e.Message}", e);
        }
    }

    public override void Write(Utf8JsonWriter writer, IMatchingRule value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", RuleTypes.First(x => x.Value == value.GetType()).Key);
        foreach (var property in JsonSerializer.SerializeToElement(value, value.GetType(), options).EnumerateObject())
        {
            property.WriteTo(writer);
        }
        writer.WriteEndObject();
    }
}
