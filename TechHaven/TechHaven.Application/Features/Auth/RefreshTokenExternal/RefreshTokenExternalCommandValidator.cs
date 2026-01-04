using FluentValidation;

namespace TechHaven.Application.Features.Auth.RefreshTokenExternal;

public class RefreshTokenExternalCommandValidator : AbstractValidator<RefreshTokenExternalCommand>
{
    public RefreshTokenExternalCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");

        RuleFor(x => x.EncryptedDbConfig)
            .NotEmpty().WithMessage("Database configuration is required");
    }
}