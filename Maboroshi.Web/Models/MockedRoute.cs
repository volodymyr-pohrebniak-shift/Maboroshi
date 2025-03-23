namespace Maboroshi.Web.Models;

using Converters;
using Utils;
using System.Text.Json.Serialization;

[method: JsonConstructor]
public class MockedRoute(
    string urlTemplate,
    HttpMethod httpMethod,
    IEnumerable<MockedRouteResponse> responses,
    ResponseSelectionStrategy responseSelectionStrategy,
    bool enabled)
{
    public string UrlTemplate { get; } = Guard.Against.NullOrWhiteSpace(urlTemplate, nameof(urlTemplate));

    [JsonConverter(typeof(JsonHttpMethodConverter))]
    public HttpMethod HttpMethod { get; } = httpMethod;

    public IEnumerable<MockedRouteResponse> Responses { get; } = Guard.Against.Null(responses, nameof(responses));

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ResponseSelectionStrategy ResponseSelectionStrategy { get; } = responseSelectionStrategy;

    public bool Enabled { get; } = enabled;
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