using FluentValidation;

namespace TechHaven.Application.Features.Reports.Queries.GetCommissionReport;

public class GetTopSellingProductsValidator : AbstractValidator<GetCommissionReportQuery>
{
  public GetTopSellingProductsValidator()
  {
    RuleFor(x => x.Month)
    .NotEmpty().WithMessage("Month is required")
    .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");

    RuleFor(x => x.Year)
    .NotEmpty().WithMessage("Year is required")
    .InclusiveBetween(2000, DateTime.UtcNow.Year).WithMessage($"Year must be between 2000 and {DateTime.UtcNow.Year}");   
  }
}