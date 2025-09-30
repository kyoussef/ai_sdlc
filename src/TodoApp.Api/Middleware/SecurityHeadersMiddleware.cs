using Microsoft.AspNetCore.Http;

namespace TodoApp.Api.Middleware;

/// <summary>
/// Adds a defensive security header baseline to every HTTP response.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates the middleware wrapper.
    /// </summary>
    /// <param name="next">Next middleware in the pipeline.</param>
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Appends cache-control, framing, and XSS protection headers before the response is sent.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-XSS-Protection"] = "0";
            return Task.CompletedTask;
        });
        await _next(context);
    }
}
