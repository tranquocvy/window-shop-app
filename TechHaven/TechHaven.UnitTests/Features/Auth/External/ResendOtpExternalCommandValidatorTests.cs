using FluentValidation;
using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.ResendOtpExternal;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

// Mock validator class nếu bạn chưa có file thật, hoặc dùng file thật của bạn
public class ResendOtpExternalCommandValidator : AbstractValidator<ResendOtpExternalCommand>
{
    public ResendOtpExternalCommandValidator()
    {
        RuleFor(x => x.OtpSessionId)
            .NotEmpty().WithMessage("Session ID required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Invalid GUID");

        RuleFor(x => x.EncryptedDbConfig)
            .NotEmpty().WithMessage("Database configuration is required");
    }
}

[ExcludeFromCodeCoverage]
public class ResendOtpExternalCommandValidatorTests
{
    private readonly ResendOtpExternalCommandValidator _validator;

    public ResendOtpExternalCommandValidatorTests()
    {
        _validator = new ResendOtpExternalCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new ResendOtpExternalCommand(Guid.NewGuid().ToString(), "some_base64_config");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MissingConfig_ShouldFail()
    {
        var command = new ResendOtpExternalCommand(Guid.NewGuid().ToString(), "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EncryptedDbConfig);
    }
}