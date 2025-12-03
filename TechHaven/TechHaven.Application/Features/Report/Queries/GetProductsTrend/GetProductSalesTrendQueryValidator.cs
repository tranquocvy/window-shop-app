using FluentValidation;

namespace TechHaven.Application.Features.Reports.Queries.GetProductsTrend;

public class GetProductSalesTrendQueryValidator : AbstractValidator<GetProductSalesTrendQuery>
{
  public GetProductSalesTrendQueryValidator()
  {
    RuleFor(x => x.ProductId)
      .NotEmpty().WithMessage("Product id is required")
      .GreaterThan(0).WithMessage("Product id must be a positive integer");

    RuleFor(x => x.StartDate)
      .NotEmpty().WithMessage("Start date is required")
      .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date");

    RuleFor(x => x.EndDate)
      .NotEmpty().WithMessage("End date is required")
      .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date");

    RuleFor(x => x.PeriodType)
      .IsInEnum().WithMessage("Invalid period type");
  }
}