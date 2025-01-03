namespace Maboroshi.Web.Models;

using Maboroshi.Web.Converters;
using Maboroshi.Web.Utils;
using System.Text.Json.Serialization;

[method: JsonConstructor]
public class MockedRoute(string url, HttpMethod httpMethod, IEnumerable<MockedRouteResponse> responses, ResponseSelectionStrategy responseSelectionStrategy)
{
    public string Url { get; } = Guard.Against.NullOrWhiteSpace(url, nameof(url));
    [JsonConverter(typeof(JsonHttpMethodConverter))]
    public HttpMethod HttpMethod { get; } = httpMethod;
    public IEnumerable<MockedRouteResponse> Responses { get; } = Guard.Against.NullOrEmpty(responses, nameof(responses));
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResponseSelectionStrategy ResponseSelectionStrategy { get; } = responseSelectionStrategy;
}

public enum ResponseSelectionStrategy
{
    Default,
    Random,
    Sequence
}

[Flags]
public enum HttpMethod
{
    NONE = 0,
    GET = 1,
    POST = 2,
    PUT = 4,
    DELETE = 8,
    PATCH = 16
}