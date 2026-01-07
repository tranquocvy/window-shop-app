using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.Login;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new LoginCommand("admin", "password123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyFields_ShouldFail()
    {
        var command = new LoginCommand("", "");
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldFail()
    {
        var command = new LoginCommand("admin", "12345"); // 5 chars (min 6)
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 6 characters long.");
    }
}