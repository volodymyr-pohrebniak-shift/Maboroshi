namespace Maboroshi.Web.RouteMatching;

public class UrlMatchinStrategyFactory
{
    public IUrlMatchingStrategy Create(string template)
    {
        return new AspNetCoreUrlMatchingStrategy(template);
    }
}