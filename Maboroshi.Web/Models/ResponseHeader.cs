using Maboroshi.Web.Utils;
using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models;

[method: JsonConstructor]
public class ResponseHeader(string key, string value)
{
    public string Key { get; } = Guard.Against.NullOrWhiteSpace(key, nameof(key));
    public string Value { get; } = value;
}
