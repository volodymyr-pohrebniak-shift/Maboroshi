namespace Maboroshi.Web.Converters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Maboroshi.Web.Models;

public class JsonHttpMethodConverter : JsonConverter<HttpMethod>
{
    public override HttpMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var rawValue = reader.GetString();

        if (string.IsNullOrEmpty(rawValue))
        {
            return HttpMethod.NONE;
        }

        HttpMethod result = HttpMethod.NONE;

        var methods = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var method in methods)
        {
            if (Enum.TryParse<HttpMethod>(method.Trim(), true, out var parsedValue))
            {
                result |= parsedValue;
            }
            else
            {
                throw new JsonException($"Invalid HttpMethod: {method}");
            }
        }

        return result;
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
