using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace TodoApp.Api.Middleware;

/// <summary>
/// Ensures every request/response pair has a stable correlation identifier propagated via <c>X-Request-ID</c> header.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Request-ID";
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates a new <see cref="CorrelationIdMiddleware"/> decorator.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Reads or generates a correlation identifier and injects it onto the response and activity scope.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var cid) || string.IsNullOrWhiteSpace(cid))
        {
            cid = Guid.NewGuid().ToString("N");
        }

        // Mirror the identifier onto the response so downstream services can gather it without custom code.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = cid!;
            return Task.CompletedTask;
        });

        using var activity = new Activity("request").Start();
        activity?.AddTag("request.id", cid!);
        context.Items[HeaderName] = cid!.ToString();
        await _next(context);
    }
}
