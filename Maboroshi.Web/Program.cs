using Maboroshi.Web.RouteMatching;
using Maboroshi.Web.Templates;

namespace Maboroshi.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IMockedRouteStore>((sp) =>
        {
            var root = new FileConfigurationParser(Path.Combine(builder.Environment.ContentRootPath, "mocks/example.json")).Parse();
            return new InMemoryMockedRouteStore(root!.Environments.First().Routes, new UrlMatchingHandler(new UrlMatchinStrategyFactory()));
        });

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<RequestProcessor>();
        builder.Services.AddSingleton<TemplateResolver>();

        var app = builder.Build();

        app.Map("{**catchAll}", (HttpContext context, RequestProcessor requestProcessor) => requestProcessor.Process(context));

        app.Run();
    }
}