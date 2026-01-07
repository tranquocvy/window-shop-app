using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.ResendOtp;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.General;

[ExcludeFromCodeCoverage]
public class ResendOtpCommandValidatorTests
{
    private readonly ResendOtpCommandValidator _validator;

    public ResendOtpCommandValidatorTests()
    {
        _validator = new ResendOtpCommandValidator();
    }

    [Fact]
    public void Validate_ValidGuid_ShouldPass()
    {
        var command = new ResendOtpCommand(Guid.NewGuid().ToString());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptySessionId_ShouldFail()
    {
        var command = new ResendOtpCommand("");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OtpSessionId)
              .WithErrorMessage("OTP session ID is required");
    }

    [Fact]
    public void Validate_InvalidGuidFormat_ShouldFail()
    {
        var command = new ResendOtpCommand("not-a-guid-string");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OtpSessionId)
              .WithErrorMessage("OTP session ID must be a valid GUID");
    }
}