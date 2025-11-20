using FluentValidation;

namespace TechHaven.Application.Features.Auth.ResendOtp;

public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
  public ResendOtpCommandValidator()
  {
    RuleFor(x => x.OtpSessionId)
        .NotEmpty().WithMessage("OTP session ID is required")
        .Must(BeValidGuid).WithMessage("OTP session ID must be a valid GUID");
  }

  private bool BeValidGuid(string sessionId)
  {
    return Guid.TryParse(sessionId, out _);
  }
}