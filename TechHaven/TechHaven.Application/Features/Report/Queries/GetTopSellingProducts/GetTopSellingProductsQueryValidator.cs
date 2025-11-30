using FluentValidation;

namespace TechHaven.Application.Features.Reports.Queries.GetTopSellingProducts;

public class GetTopSellingProductsValidator : AbstractValidator<GetTopSellingProductsQuery>
{
  public GetTopSellingProductsValidator()
  {
    RuleFor(x => x.StartDate)
      .NotEmpty().WithMessage("Start date is required")
      .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

    RuleFor(x => x.EndDate)
      .NotEmpty().WithMessage("End date is required")
      .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");
    
    RuleFor(x => x.TopCount)
      .GreaterThanOrEqualTo(0).WithMessage("Top Count cannot be negative");
  }
}