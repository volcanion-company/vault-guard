using System.Diagnostics;

namespace VaultGuard.Api.Middleware;

/// <summary>
/// Middleware that logs HTTP request and response details, including request method, path, status code, duration, and
/// user information. Adds a unique request identifier to each response for traceability.
/// </summary>
/// <remarks>The middleware adds an "X-Request-Id" header to each response and stores the request ID in the
/// HttpContext for downstream access. User information is logged if available in the context. All exceptions are logged
/// before being rethrown. This middleware should be registered early in the pipeline to ensure comprehensive
/// logging.</remarks>
/// <param name="next">The next middleware delegate in the HTTP request pipeline. Invoked after logging the request start information.</param>
/// <param name="logger">The logger used to record request and response details, errors, and diagnostic information.</param>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>
    /// Processes an HTTP request by assigning a unique request identifier, logging request and response details, and
    /// invoking the next middleware in the pipeline asynchronously.
    /// </summary>
    /// <remarks>Adds an "X-Request-Id" header to the response and stores the request identifier in <see
    /// cref="HttpContext.Items"/>. Logs request start, completion, and any errors encountered during processing. This
    /// method should be used as part of an ASP.NET Core middleware pipeline.</remarks>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response. Must not be null.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing the request
        var stopwatch = Stopwatch.StartNew();
        // Generate a unique request ID
        var requestId = Guid.NewGuid().ToString();
        // Add request ID to response headers
        context.Response.Headers.Append("X-Request-Id", requestId);
        // Store request ID in HttpContext for downstream access
        context.Items["RequestId"] = requestId;

        try
        {
            // Log request start information (PII removed for compliance)
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("HTTP {Method} {Path} started - RequestId={RequestId}", 
                    context.Request.Method, 
                    context.Request.Path, 
                    requestId);

            // Invoke the next middleware in the pipeline
            await next(context);

            // Stop timing the request
            stopwatch.Stop();

            // Log request completion information
            logger.LogInformation(
                "HTTP {Method} {Path} completed - StatusCode={StatusCode}, Duration={Duration}ms, RequestId={RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);
        }
        catch (Exception ex)
        {
            // Handle exceptions by logging error details
            // Stop timing the request
            stopwatch.Stop();

            // Log error information
            if (logger.IsEnabled(LogLevel.Error))
                logger.LogError(ex, "HTTP {Method} {Path} failed - Duration={Duration}ms, RequestId={RequestId}", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds, requestId);

            // Rethrow the exception to propagate it up the pipeline
            throw;
        }
    }
}
