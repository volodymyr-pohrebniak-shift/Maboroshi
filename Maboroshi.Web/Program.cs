using Maboroshi.Web.RouteMatching;
using Maboroshi.Web.Services;
using Maboroshi.Web.Templates;

namespace Maboroshi.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<UrlMatchinStrategyFactory>();
        builder.Services.AddSingleton<IUrlMatchingHandler, UrlMatchingHandler>();
        builder.Services.AddSingleton<IMockedRouteStore, InMemoryMockedRouteStore>();
        builder.Services.AddSingleton<ICacheResetTokenProvider, CacheResetTokenProvider>();

        builder.Services.AddSingleton((IServiceProvider serviceProvider) =>
        {
            var root = new FileConfigurationParser(Path.Combine(builder.Environment.ContentRootPath, "mocks/example.json")).Parse();
            var mockService = serviceProvider.GetRequiredService<IMockedRouteStore>();
            var provider = new EnvironmentsProvider(mockService);
            provider.SetEnvironments([.. root!.Environments]);
            return provider;
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<RequestProcessor>();
        builder.Services.AddSingleton<TemplateResolver>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapFallbackToFile("index.html");

        // load config from file
        app.Services.GetRequiredService<EnvironmentsProvider>();

        app.MapGet("/$$$SYSTEM$$$/environments", (EnvironmentsProvider environmentsProvider) => environmentsProvider.GetEnvironments());

        app.MapPut("/$$$SYSTEM$$$/environments", (Models.Environment[] environtments, EnvironmentsProvider environmentsProvider, ICacheResetTokenProvider tokenProvider) => {
            environmentsProvider.SetEnvironments(environtments);
            tokenProvider.CancelAndRefresh();
            Results.Ok();
        });

        app.Map("{**catchAll}", async (HttpContext context, RequestProcessor requestProcessor) => await requestProcessor.Process(context));

        app.Run();
    }
}

class EnvironmentsProvider(IMockedRouteStore routesStore)
{
    private List<Models.Environment> _environments = [];

    public void SetEnvironments(Models.Environment[] environments)
    {
        _environments = [.. environments];

        if (_environments.Count > 0)
        {
            var activeEnv = _environments.FirstOrDefault(x => x.IsActive) ?? _environments[0];
            routesStore.SetRoutes(activeEnv.Routes);
        }
    }

    public IEnumerable<Models.Environment> GetEnvironments()
    {
        return _environments;
    }
}