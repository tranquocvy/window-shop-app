using System.Net;
using System.Text.Json;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ResponseWrapper<object>
        {
            Success = false
        };

        switch (exception)
        {
            case ValidationException validationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Validation failed";

                // Chuyển đổi Dictionary<string, string[]> thành List<string>
                response.Errors = validationException.Errors
                    .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
                    .ToList();

                _logger.LogWarning(
                    "Validation failed: {Errors}",
                    string.Join("; ", response.Errors));
                break;

            case NotFoundException notFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundException.Message;
                response.Errors = new List<string> { notFoundException.Message };

                _logger.LogWarning("Not found: {Message}", notFoundException.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An error occurred while processing your request";
                response.Errors = new List<string> { exception.Message };

                _logger.LogError(
                    exception,
                    "Unhandled exception: {Message}",
                    exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}