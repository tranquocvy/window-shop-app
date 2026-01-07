using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.Signup;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class SignupCommandValidatorTests
{
    private readonly SignupCommandValidator _validator;

    public SignupCommandValidatorTests()
    {
        _validator = new SignupCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new SignupCommand(
            "Nguyen Van A",
            "nguyenvana@test.com",
            "nguyen_van_a", // Valid regex chars
            "password123",
            2
        );

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MissingRequiredFields_ShouldFail()
    {
        var command = new SignupCommand("", "", "", "", 1);
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserFullName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.UserName);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory] // Test Regex Username: chỉ cho phép chữ, số và gạch dưới
    [InlineData("user@name")] // Invalid char @
    [InlineData("user-name")] // Invalid char -
    [InlineData("user name")] // Invalid space
    [InlineData("ab")]        // Too short (min 3)
    public void Validate_InvalidUserNameFormat_ShouldFail(string invalidUserName)
    {
        var command = new SignupCommand("Test", "t@t.com", invalidUserName, "pass123", 1);
        var result = _validator.TestValidate(command);

        // Check lỗi Regex hoặc Length
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var command = new SignupCommand("Test", "invalid-email", "user123", "pass123", 1);
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldFail()
    {
        var command = new SignupCommand("Test", "t@t.com", "user123", "12345", 1); // 5 chars
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 6 characters");
    }
}