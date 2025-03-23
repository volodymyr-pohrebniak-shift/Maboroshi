namespace Maboroshi.Web.Converters;

using Maboroshi.Web.Models.MatchingRules;
using System.Text.Json.Serialization;
using System.Text.Json;

public class JsonMatchingRuleConverter : JsonConverter<IMatchingRule>
{
    public override IMatchingRule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            var jsonObject = jsonDoc.RootElement;

            if (jsonObject.TryGetProperty("operation", out var opProp) && Enum.TryParse<AggregateRuleOperation>(opProp.GetString()!, true, out var operation))
            {
                var rulesJson = jsonObject.GetProperty("rules").GetRawText();

                var rules = JsonSerializer.Deserialize<IEnumerable<IMatchingRule>>(rulesJson, options)
                            ?? [];

                return new AggregateRule(rules, operation);
            }
            else if (jsonObject.TryGetProperty("type", out var typeProp))
            {
                var type = typeProp.GetString();

                return type switch
                {
                    "Header" => JsonSerializer.Deserialize<HeaderMatchingRule>(jsonObject.GetRawText(), options)!,
                    "Query" => JsonSerializer.Deserialize<QueryMatchingRule>(jsonObject.GetRawText(), options)!,
                    "Route" => JsonSerializer.Deserialize<RouteMatchingRule>(jsonObject.GetRawText(), options)!,
                    _ => throw new NotSupportedException($"Rule type '{type}' is not supported.")
                };
            }
        }
        throw new JsonException("Invalid JSON for rule deserialization.");
    }

    public override void Write(Utf8JsonWriter writer, IMatchingRule value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        /*
        switch (value)
        {
            case SingleOperationMatchingRule singleRule:
                writer.WriteString("type", value.GetType().Name);
                writer.WriteString("key", singleRule.Key);
                writer.WriteString("value", singleRule.Value);
                writer.WriteBoolean("negate", singleRule.Negate);
                writer.WriteString("operation", singleRule.Operation.ToString());
                break;

            case AggregateRule aggregateRule:
                writer.WriteString("type", nameof(AggregateRule));
                writer.WritePropertyName("rules");
                JsonSerializer.Serialize(writer, aggregateRule.rules, options);
                writer.WriteString("op", aggregateRule.op.ToString());
                break;
        }
        */
        writer.WriteEndObject();
    }
}
