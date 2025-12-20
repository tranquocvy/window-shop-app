using FluentValidation;

namespace TechHaven.Application.Features.AppSetting.Commands.UpdateAppSetting;

public class UpdateAppSettingCommandValidator : AbstractValidator<UpdateAppSettingCommand>
{
    public UpdateAppSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key cannot exceed 100 characters.");

        RuleFor(x => x.Value)
            .MaximumLength(1000).WithMessage("Value cannot exceed 1000 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description cannot exceed 255 characters.");
    }
}