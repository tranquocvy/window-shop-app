using FluentValidation;

namespace TechHaven.Application.Features.Auth.VerifyOtp;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
  public VerifyOtpCommandValidator()
  {
    RuleFor(x => x.UserId)
        .GreaterThan(0).WithMessage("User ID must be greater than 0");

    RuleFor(x => x.OtpCode)
        .NotEmpty().WithMessage("OTP code is required")
        .Length(6).WithMessage("OTP code must be 6 digits")
        .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits");
  }
}