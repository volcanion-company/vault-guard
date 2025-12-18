using System.Net;
using System.Text.Json;
using VaultGuard.Application.Exceptions;

namespace VaultGuard.Api.Middleware;


/// <summary>
/// Middleware that provides centralized exception handling for HTTP requests in the application pipeline.
/// </summary>
/// <remarks>This middleware intercepts unhandled exceptions thrown during request processing and returns a
/// standardized JSON error response. It maps specific exception types, such as not found or validation errors, to
/// appropriate HTTP status codes and response formats. Place this middleware early in the pipeline to ensure consistent
/// error handling across the application.</remarks>
/// <param name="next">The next middleware delegate in the HTTP request pipeline. This delegate is invoked to continue processing the
/// request.</param>
/// <param name="logger">The logger used to record unhandled exceptions and related error information.</param>
/// <param name="environment">The hosting environment to determine if running in production.</param>
public class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment environment)
{
    /// <summary>
    /// Processes an HTTP request and handles any unhandled exceptions that occur during the request pipeline.
    /// </summary>
    /// <remarks>If an unhandled exception occurs during request processing, the exception is logged and an
    /// appropriate error response is generated. This method should be used as part of the ASP.NET Core middleware
    /// pipeline.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next middleware in the pipeline
            await next(context);
        }
        catch (Exception ex)
        {
            // Log the exception
            logger.LogError(ex, "An unhandled exception occurred");
            // Handle the exception and generate an appropriate response
            await HandleExceptionAsync(context, ex, environment);
        }
    }

    /// <summary>
    /// Handles an exception by generating an appropriate JSON response and setting the HTTP status code on the
    /// response.
    /// </summary>
    /// <remarks>The response content type is set to "application/json". The status code and response body are
    /// determined based on the type of exception provided. For known exception types such as NotFoundException and
    /// ValidationException, specific status codes and messages are returned. For all other exceptions, a generic
    /// internal server error response is generated. In production, internal error details are hidden.</remarks>
    /// <param name="context">The HTTP context for the current request. The response will be written to this context.</param>
    /// <param name="exception">The exception to handle. Determines the status code and error details included in the response.</param>
    /// <param name="environment">The hosting environment to determine if running in production.</param>
    /// <returns>A task that represents the asynchronous operation of writing the error response.</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment environment)
    {
        // Set the response content type to JSON
        context.Response.ContentType = "application/json";
        // Determine the response based on the exception type
        object response = exception switch
        {
            // Handle NotFoundException
            NotFoundException notFoundException => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                message = notFoundException.Message,
                details = (string?)null,
            },
            // Handle ValidationException
            ValidationException validationException => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "One or more validation errors occurred.",
                details = (object)validationException.Errors
            },
            UnauthorizedAccessException unauthorizedAccessException => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                message = unauthorizedAccessException.Message,
                details = (string?)null,
            },
            // Handle all other exceptions
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "An error occurred while processing your request.",
                // Hide internal details in production
                details = environment.IsProduction() ? (object?)null : exception.Message,
                requestId = context.Items["RequestId"]?.ToString()
            }
        };
        // Set the appropriate status code
        var statusCode = exception switch
        {
            // Handle NotFoundException
            NotFoundException => (int)HttpStatusCode.NotFound,
            // Handle ValidationException
            ValidationException => (int)HttpStatusCode.BadRequest,
            // Handle all other exceptions
            _ => (int)HttpStatusCode.InternalServerError
        };
        // Write the response
        context.Response.StatusCode = statusCode;
        // Serialize the response object to JSON
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        // Write the JSON response to the HTTP response body
        await context.Response.WriteAsync(jsonResponse);
    }
}
