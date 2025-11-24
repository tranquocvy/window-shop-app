using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace TechHaven.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that logs detailed information about requests being handled.
/// Includes timing, request parameters, and results.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    var stopwatch = Stopwatch.StartNew();

    // Log request start with parameters (exclude sensitive data)
    _logger.LogInformation(
        "Handling {RequestName} with parameters: {@RequestData}",
        requestName,
        SanitizeRequest(request));

    try
    {
      var response = await next();
      stopwatch.Stop();

      // Log successful completion with timing
      _logger.LogInformation(
          "Completed {RequestName} in {ElapsedMilliseconds}ms",
          requestName,
          stopwatch.ElapsedMilliseconds);

      // Log performance warning if slow
      if (stopwatch.ElapsedMilliseconds > 3000) // 3 seconds threshold
      {
        _logger.LogWarning(
            "SLOW REQUEST: {RequestName} took {ElapsedMilliseconds}ms",
            requestName,
            stopwatch.ElapsedMilliseconds);
      }

      return response;
    }
    catch (Exception ex)
    {
      stopwatch.Stop();

      // Log error with full context
      _logger.LogError(
          ex,
          "Error handling {RequestName} after {ElapsedMilliseconds}ms. Request: {@RequestData}",
          requestName,
          stopwatch.ElapsedMilliseconds,
          SanitizeRequest(request));

      throw;
    }
  }

  /// <summary>
  /// Remove sensitive data from request before logging
  /// </summary>
  private object SanitizeRequest(TRequest request)
  {
    try
    {
      var json = JsonSerializer.Serialize(request);
      var sanitized = json
          .Replace("\"Password\":\"", "\"Password\":\"***")
          .Replace("\"PasswordHash\":\"", "\"PasswordHash\":\"***")
          .Replace("\"RefreshToken\":\"", "\"RefreshToken\":\"***")
          .Replace("\"OtpCode\":\"", "\"OtpCode\":\"***");

      return JsonSerializer.Deserialize<object>(sanitized) ?? request;
    }
    catch
    {
      return new { Type = typeof(TRequest).Name, Message = "Could not serialize request" };
    }
  }
}