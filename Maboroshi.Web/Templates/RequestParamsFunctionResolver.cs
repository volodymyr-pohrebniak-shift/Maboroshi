using Maboroshi.TemplateEngine;
using Maboroshi.TemplateEngine.FunctionResolvers;

namespace Maboroshi.Web.Templates;

public class RequestParamsFunctionResolver(RequestAdapter adapter) : IFunctionResolver
{
    public ReturnType? TryResolve(string functionName, params ReturnType[] additionalArguments)
    {
        return functionName.ToLowerInvariant() switch
        {
            "queryparam" => ResolveWithParams(adapter.GetQueryParam, additionalArguments),
            "urlparam" => ResolveWithParams(adapter.GetUrlParam, additionalArguments),
            "cookieparam" => ResolveWithParams(adapter.GetCookieParam, additionalArguments),
            "headerparam" =>ResolveWithParams(adapter.GetHeaderParam, additionalArguments),
            "hostname" => Resolve(adapter.GetHostname),
            "ip" => Resolve(adapter.GetIp),
            "method" => Resolve(adapter.GetMethod),
            _ => null
        };
    }

    private StringReturn ResolveWithParams(Func<string, string, string> fetchFunc, ReturnType[] arguments)
    {
        if (arguments.Length == 0 || arguments[0] is not StringReturn paramName)
        {
            throw new TemplateRuntimeException("Parameter name is required.");
        }

        string paramKey = paramName.Value;
        string defaultValue = arguments.Length > 1 ? arguments[1].ToString() : "";
        return new StringReturn(fetchFunc(paramKey, defaultValue));
    }

    private static StringReturn Resolve(Func<string> fetchFunc)
    {
        return new StringReturn(fetchFunc());
    }
}
