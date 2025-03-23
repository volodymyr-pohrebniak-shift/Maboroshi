using Maboroshi.Web.Models.MatchingRules;
using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models;

[method: JsonConstructor]
public class MockedRouteResponse(int statusCode, string name, string description, string? body, IEnumerable<BaseMatchingRule>? rules, IEnumerable<ResponseHeader>? headers, bool allowResponseCaching)
{
    public int StatusCode { get; } = statusCode;
    public string Name { get; } = name;
    public string? Description { get; } = description;
    public string? Body { get; } = body;
    public IEnumerable<BaseMatchingRule> Rules { get; } = rules ?? [];
    public IEnumerable<ResponseHeader> Headers { get; } = headers ?? [];
    public bool AllowResponseCaching { get; } = allowResponseCaching;
}
