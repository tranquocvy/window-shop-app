using System.Data;
using FluentValidation;

namespace TechHaven.Application.Features.Auth.LoginExternal;

public class LoginExternalCommandValidator : AbstractValidator<LoginExternalCommand>
{
  public LoginExternalCommandValidator()
  {
    RuleFor(x => x.UserName)
        .NotEmpty().WithMessage("Username is required.")
        .MaximumLength(50).WithMessage("Username must be at most 50 characters long.");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required.")
        .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
  }
}