using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TodoApp.Api.Middleware;

/// <summary>
/// Converts exceptions bubbling out of the pipeline into RFC7807 <c>application/problem+json</c> responses.
/// </summary>
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    /// <summary>
    /// Initializes the middleware wrapper.
    /// </summary>
    /// <param name="next">Next component in the chain.</param>
    /// <param name="logger">Logger capturing unhandled exceptions.</param>
    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Handles exceptions and writes standardized problem details responses.
    /// </summary>
    /// <param name="context">Active HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await WriteProblem(context, HttpStatusCode.Conflict, "Update conflict. The resource was modified by another process.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, HttpStatusCode status, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        var problem = new
        {
            type = "about:blank",
            title = status.ToString(),
            status = (int)status,
            detail,
            traceId = context.TraceIdentifier
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
