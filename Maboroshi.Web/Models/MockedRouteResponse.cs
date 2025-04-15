using Maboroshi.Web.Models.MatchingRules;

namespace Maboroshi.Web.Models;

public class MockedRouteResponse
{
    public int StatusCode { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Body { get; init; }
    public IEnumerable<BaseMatchingRule> Rules { get; init; } = [];
    public IEnumerable<ResponseHeader> Headers { get; init; } = [];
    public bool StrictTemplateErrors { get; init; }
    public bool DisableTemplating { get; init; }
    public int Delay { get; init; }
}
