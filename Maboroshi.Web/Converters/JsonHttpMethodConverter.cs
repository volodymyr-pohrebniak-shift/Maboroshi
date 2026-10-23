namespace Maboroshi.Web.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Maboroshi.Web.Models;

public class JsonHttpMethodConverter : JsonConverter<HttpMethod>
{
    public override HttpMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            HttpMethod result = HttpMethod.NONE;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var item = reader.GetString();
                    if (Enum.TryParse<HttpMethod>(item?.Trim(), true, out var parsed))
                        result |= parsed;
                    else
                        throw new JsonException($"Invalid HttpMethod: {item}");
                }
            }

            return result;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();

            if (string.IsNullOrEmpty(rawValue))
                return HttpMethod.NONE;

            HttpMethod result = HttpMethod.NONE;

            foreach (var method in rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse<HttpMethod>(method.Trim(), true, out var parsed))
                    result |= parsed;
                else
                    throw new JsonException($"Invalid HttpMethod: {method}");
            }

            return result;
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, HttpMethod value, JsonSerializerOptions options)
    {
        var enumValues = value
            .ToString()
            .Split(", ")
            .Select(v => v.ToUpper());

        writer.WriteStringValue(string.Join(", ", enumValues));
    }
}
