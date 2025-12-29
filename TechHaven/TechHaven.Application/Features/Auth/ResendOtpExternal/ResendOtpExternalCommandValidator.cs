using FluentValidation;

namespace TechHaven.Application.Features.Auth.ResendOtpExternal;

public class ResendOtpExternalCommandValidator : AbstractValidator<ResendOtpExternalCommand>
{
  public ResendOtpExternalCommandValidator()
  {
    RuleFor(x => x.OtpSessionId)
        .NotEmpty().WithMessage("OTP session ID is required")
        .Must(BeValidGuid).WithMessage("OTP session ID must be a valid GUID");
        RuleFor(x => x.EncryptedDbConfig)
            .NotEmpty().WithMessage("Database configuration is required")
            .MinimumLength(10).WithMessage("Database configuration seems too short to be valid");
    }

  private bool BeValidGuid(string sessionId)
  {
    return Guid.TryParse(sessionId, out _);
  }
}