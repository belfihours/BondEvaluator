using System.Net;
using System.Text.Json;
using BondEvaluator.Application.Exceptions;

namespace BondEvaluator.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode statusCode;
        if (exception is ArgumentException 
            or ArgumentNullException 
            or BondParserException)
        {
            _logger.LogError(exception, "Error while attempting retrieving data, user may have inserted wrong input.");
            statusCode = HttpStatusCode.BadRequest;
        }
        else
        {
            _logger.LogError(exception, "Error while attempting retrieving data, unhandled exception.");
            statusCode = HttpStatusCode.InternalServerError;
        }

        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            error = exception.Message
        };

        var serializedError = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(serializedError);
    }
}