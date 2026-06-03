using FluentValidation;
using SaaSify.Domain.Exceptions;
using System.Text.Json;

namespace SaaSify.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var response = new { error = "Validation failed", errors, errorCode = 400 };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }
        catch (DomainException ex)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status422UnprocessableEntity,
                ex.Message,
                422);
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(
                context,
                StatusCodes.Status408RequestTimeout,
                "Request was cancelled.",
                408);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. TraceId: {TraceId}",
                context.TraceIdentifier);

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                500,
                ex);
        }
    }

    private async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string error,
        int errorCode, // Quitarlo en un futuro por que es redundante, y usar directamente statusCode en la response
        Exception? exception = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        object response;

        if (_environment.IsDevelopment() && exception is not null)
        {
            response = new
            {
                error,
                errorCode,
                traceId = context.TraceIdentifier,
                details = exception.StackTrace
            };
        }
        else
        {
            response = new
            {
                error,
                errorCode
            };
        }

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}