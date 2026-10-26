using System.Runtime.CompilerServices;
using Maboroshi.Web.Models;
using Maboroshi.TemplateEngine;

namespace Maboroshi.Web.Templates;

public class TemplateResolver
{
    // the parsed template of each configured response; responses are replaced with new objects on every configuration update,
    // so templates of the old configuration go away together with it
    private readonly ConditionalWeakTable<MockedRouteResponse, Template> _templates = [] ;

    // the body is rendered on every request: it may depend on the request, on fake data or on the current time
    public string? RenderBody(RequestAdapter requestAdapter, MockedRouteResponse response)
    {
        if (string.IsNullOrEmpty(response.Body) || response.DisableTemplating)
            return response.Body;

        var template = _templates.GetValue(response, r => TemplateGenerator.CreateTemplate(r.Body!, new(r.StrictTemplateErrors)));
        return template.Compile(new RequestParamsFunctionResolver(requestAdapter));
    }
}
