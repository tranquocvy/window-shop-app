using FluentValidation;

namespace TechHaven.Application.Features.Auth.Signup;

public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
  public SignupCommandValidator()
  {
    RuleFor(x => x.UserFullName)
        .NotEmpty().WithMessage("Full name is required")
        .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("Email is required")
        .EmailAddress().WithMessage("Invalid email format")
        .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

    RuleFor(x => x.UserName)
        .NotEmpty().WithMessage("Username is required")
        .MinimumLength(3).WithMessage("Username must be at least 3 characters")
        .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
        .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

    RuleFor(x => x.Password)
        .NotEmpty().WithMessage("Password is required")
        .MinimumLength(6).WithMessage("Password must be at least 6 characters");

    // RuleFor(x => x.ConfirmPassword)
    //     .NotEmpty().WithMessage("Confirm password is required")
    //     .Equal(x => x.Password).WithMessage("Passwords do not match");
  }
}