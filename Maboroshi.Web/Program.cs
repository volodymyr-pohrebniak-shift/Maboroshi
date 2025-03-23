using Maboroshi.Web.Models;
using Maboroshi.Web.RouteMatching;
using Maboroshi.Web.Templates;

namespace Maboroshi.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var root = new FileConfigurationParser(Path.Combine(builder.Environment.ContentRootPath, "mocks/example.json")).Parse();

        builder.Services.AddSingleton<IMockedRouteStore>((_) => new InMemoryMockedRouteStore(root!.Environments.First().Routes, new UrlMatchingHandler(new UrlMatchinStrategyFactory())));

        builder.Services.AddSingleton((_) => new EnvironmentsProvider(root!));

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<RequestProcessor>();
        builder.Services.AddSingleton<TemplateResolver>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapGet("/$$$SYSTEM$$$/environments", (EnvironmentsProvider environmentsProvider) => environmentsProvider.GetEnvironments());

        app.Map("{**catchAll}", (HttpContext context, RequestProcessor requestProcessor) => requestProcessor.Process(context));

        //app.MapFallbackToFile("/index.html");

        app.Run();
    }
}

class EnvironmentsProvider(Root root)
{
    private Root root = root;

    public IEnumerable<Models.Environment> GetEnvironments()
    {
        return root.Environments;
    }
}