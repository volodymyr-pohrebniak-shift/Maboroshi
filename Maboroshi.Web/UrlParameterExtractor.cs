using Microsoft.AspNetCore.Routing.Template;

namespace Maboroshi.Web;

public static class UrlParameterExtractor
{
    public static Dictionary<string, string> Extract(string urlTemplate, string url)
    {
        var template = TemplateParser.Parse(urlTemplate);
        
        var templateMatcher = new TemplateMatcher(template, GetDefaultsForTemplate(template));
        
        var values = new RouteValueDictionary();

        templateMatcher.TryMatch(url, values);

        return values.ToDictionary(x=> x.Key, x=> x.Value?.ToString() ?? string.Empty);
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