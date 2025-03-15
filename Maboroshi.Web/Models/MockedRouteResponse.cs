using Maboroshi.Web.Models.MatchingRules;
using System.Text.Json.Serialization;

namespace Maboroshi.Web.Models;

[method: JsonConstructor]
public class MockedRouteResponse(int statusCode, string? body, IEnumerable<IMatchingRule>? rules, IEnumerable<ResponseHeader>? headers, bool allowResponseCaching)
{
    public int StatusCode { get; } = statusCode;
    public string? Body { get; } = body;
    public IEnumerable<IMatchingRule> Rules { get; } = rules ?? [];
    public IEnumerable<ResponseHeader> Headers { get; } = headers ?? [];
    public bool AllowResponseCaching { get; } = allowResponseCaching;
}
