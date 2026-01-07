using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Auth.RefreshTokenExternal;
using Xunit;

namespace TechHaven.UnitTests.Features.Auth.External;

[ExcludeFromCodeCoverage]
public class RefreshTokenExternalCommandValidatorTests
{
    private readonly RefreshTokenExternalCommandValidator _validator;

    public RefreshTokenExternalCommandValidatorTests()
    {
        _validator = new RefreshTokenExternalCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new RefreshTokenExternalCommand("token", "config");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_MissingFields_ShouldFail()
    {
        var command = new RefreshTokenExternalCommand("", "");
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        result.ShouldHaveValidationErrorFor(x => x.EncryptedDbConfig);
    }
}