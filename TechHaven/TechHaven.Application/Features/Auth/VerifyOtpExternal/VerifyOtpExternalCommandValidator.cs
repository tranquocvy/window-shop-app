using FluentValidation;

namespace TechHaven.Application.Features.Auth.VerifyOtpExternal;

public class VerifyOtpExternalCommandValidator : AbstractValidator<VerifyOtpExternalCommand>
{
  public VerifyOtpExternalCommandValidator()
  {
    RuleFor(x => x.OtpSessionId)
        .NotEmpty().WithMessage("OTP session ID is required")
        .Must(BeValidGuid).WithMessage("OTP session ID must be a valid GUID");

    RuleFor(x => x.OtpCode)
        .NotEmpty().WithMessage("OTP code is required")
        .Length(6).WithMessage("OTP code must be 6 digits")
        .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits");
  }

  private bool BeValidGuid(string sessionId)
  {
    return Guid.TryParse(sessionId, out _);
  }
}