using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TechHaven.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that validates commands/queries before they are handled.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;
  private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

  public ValidationBehavior(
      IEnumerable<IValidator<TRequest>> validators,
      ILogger<ValidationBehavior<TRequest, TResponse>> logger)
  {
    _validators = validators;
    _logger = logger;
  }

  public async Task<TResponse> Handle(
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken)
  {
    if (!_validators.Any())
    {
      _logger.LogDebug("No validators registered for {RequestName}", typeof(TRequest).Name);
      return await next();
    }

    var context = new ValidationContext<TRequest>(request);

    var validationResults = await Task.WhenAll(
        _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

    var failures = validationResults
        .Where(r => r.Errors.Any())
        .SelectMany(r => r.Errors)
        .ToList();

    if (failures.Any())
    {
      _logger.LogWarning(
          "Validation failed for {RequestName}. Errors: {Errors}",
          typeof(TRequest).Name,
          failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

      throw new Common.Exceptions.ValidationException(failures);
    }

    _logger.LogDebug("Validation succeeded for {RequestName}", typeof(TRequest).Name);

    return await next();
  }
}