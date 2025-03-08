using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;

namespace Maboroshi.Web.RouteMatching;

/// <summary>
/// It is used to satisfy constraint resolver
/// We do not use custom url constraints, so we don't need real service provider
/// </summary>
internal sealed class DummyServiceProvider : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        return null;
    }
}

public class AspNetCoreUrlMatchingStrategy : IUrlMatchingStrategy
{
    private readonly TemplateMatcher _templateMatcher;
    private readonly RouteTemplate _template;
    private readonly Dictionary<string, List<IRouteConstraint>> _constraints = [];

    public AspNetCoreUrlMatchingStrategy(string url)
    {
        _template = TemplateParser.Parse(url);
        
        _templateMatcher = new TemplateMatcher(_template, GetDefaultsForTemplate(_template));

        foreach (var parameter in _template.Parameters)
        {
            foreach (var inlineConstraint in parameter.InlineConstraints)
            {
                var constraintResolver = new DefaultInlineConstraintResolver(Options.Create(new RouteOptions()), new DummyServiceProvider());
                var constraint = constraintResolver.ResolveConstraint(inlineConstraint.Constraint);
                if (constraint == null) continue;
                if (_constraints.TryGetValue(parameter.Name!, out var value))
                    value.Add(constraint);
                else
                    _constraints.Add(parameter.Name!, [constraint]);
            }
        }
    }
    
    public bool IsUrlMatch(string url)
    {
        if (!url.StartsWith('/')) 
            url = url.Insert(0, "/");
        var values = new RouteValueDictionary();

        if (_templateMatcher.TryMatch(url, values))
        {
            var a = _template.Parameters
                .Where(parameter => _constraints.ContainsKey(parameter.Name!))
                .All(parameter => _constraints[parameter.Name!]
                    .All(x => x.Match(httpContext: null, route: null, parameter.Name!, values, RouteDirection.IncomingRequest)));
             
            return _template.Parameters
                .Where(parameter => _constraints.ContainsKey(parameter.Name!))
                .All(parameter => _constraints[parameter.Name!]
                    .All(x => x.Match(httpContext: null, route: null, parameter.Name!, values, RouteDirection.IncomingRequest)));
        }
        
        return false;
    }
    
    private static RouteValueDictionary GetDefaultsForTemplate(RouteTemplate parsedTemplate)
    {
        var result = new RouteValueDictionary();

        foreach (var parameter in parsedTemplate.Parameters)
        {
            if (parameter.DefaultValue != null)
            {
                result.Add(parameter.Name!, parameter.DefaultValue);
            }
        }

        return result;
    }
}