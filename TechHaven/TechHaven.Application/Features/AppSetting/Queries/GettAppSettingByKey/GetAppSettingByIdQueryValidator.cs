using FluentValidation;

namespace TechHaven.Application.Features.AppSetting.Queries.GetAppSettingByKey;

public class GetAppSettingByKeyQueryValidator : AbstractValidator<GetAppSettingByKeyQuery>
{
    public GetAppSettingByKeyQueryValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key cannot exceed 100 characters.");
    }
}