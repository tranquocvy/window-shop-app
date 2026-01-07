using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.VerifyOtp;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class VerifyOtpCommandValidatorTests
{
    private readonly VerifyOtpCommandValidator _validator;

    public VerifyOtpCommandValidatorTests()
    {
        _validator = new VerifyOtpCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        // SessionId phải là GUID hợp lệ
        var command = new VerifyOtpCommand(Guid.NewGuid().ToString(), "123456");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidSessionId_ShouldFail()
    {
        var command = new VerifyOtpCommand("not-a-guid", "123456");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OtpSessionId)
              .WithErrorMessage("OTP session ID must be a valid GUID");
    }

    [Theory]
    [InlineData("")]        // Empty
    [InlineData("12345")]   // Too short
    [InlineData("1234567")] // Too long
    [InlineData("12345a")]  // Non-digit
    public void Validate_InvalidOtpCode_ShouldFail(string invalidCode)
    {
        var command = new VerifyOtpCommand(Guid.NewGuid().ToString(), invalidCode);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OtpCode);
    }
}