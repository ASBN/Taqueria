using System.Net;
using System.Text.Json;

namespace Taqueria.Api.Middleware;
using Taqueria.Application.Exceptions;
using Taqueria.Domain.Exceptions;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensaje) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            BusinessValidationException => (HttpStatusCode.BadRequest, exception.Message),
            DomainException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado en la API.");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje }));
    }
}
