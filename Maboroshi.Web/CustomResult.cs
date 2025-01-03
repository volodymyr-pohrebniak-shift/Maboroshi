namespace Maboroshi.Web;

public class CustomResult(int statusCode, Dictionary<string, string> headers, string? content = "") : IResult
{
    public int StatusCode { get; } = statusCode;
    public Dictionary<string, string> Headers { get; } = headers ?? [];
    public string? Content { get; } = content;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;

        foreach (var header in Headers)
        {
            httpContext.Response.Headers[header.Key] = header.Value;
        }

        string contentType = Headers
            .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            .Value ?? "text/plain";

        httpContext.Response.ContentType = contentType;

        if (!string.IsNullOrEmpty(Content))
        {
            await httpContext.Response.WriteAsync(Content);
        }
    }
}
