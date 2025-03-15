namespace Maboroshi.Web;

using Maboroshi.Web.Converters;
using Maboroshi.Web.Models;
using Maboroshi.Web.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

public interface IConfigurationParser
{
    Root? Parse();
}

public class FileConfigurationParser(string filePath) : IConfigurationParser
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new JsonMatchingRuleConverter() },
        PropertyNameCaseInsensitive = true,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
    };
    private readonly string _filePath = Guard.Against.NullOrWhiteSpace(filePath);

    public Root? Parse()
    {
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Root>(json, _options);
    }
}
