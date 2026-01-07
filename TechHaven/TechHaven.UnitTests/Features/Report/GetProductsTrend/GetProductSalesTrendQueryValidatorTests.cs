using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Reports.Queries.GetProductsTrend;
using TechHaven.Shared.DTOs.Reports; // Namespace chứa Enum ReportPeriodType
using Xunit;

namespace TechHaven.UnitTests.Features.Reports.GetProductsTrend;

[ExcludeFromCodeCoverage]
public class GetProductSalesTrendQueryValidatorTests
{
    private readonly GetProductSalesTrendQueryValidator _validator;

    public GetProductSalesTrendQueryValidatorTests()
    {
        _validator = new GetProductSalesTrendQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_ShouldPass()
    {
        var query = new GetProductSalesTrendQuery(
            1,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            Shared.DTOs.Reports.ReportPeriodType.Daily
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidProductId_ShouldFail(int invalidId)
    {
        var query = new GetProductSalesTrendQuery(invalidId, DateTime.UtcNow, DateTime.UtcNow, Shared.DTOs.Reports.ReportPeriodType.Daily);
        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Validate_StartDateAfterEndDate_ShouldFail()
    {
        // Start Date lớn hơn End Date
        var query = new GetProductSalesTrendQuery(
            1,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow,
            ReportPeriodType.Daily
        );

        var result = _validator.TestValidate(query);

        // Validator check cả 2 chiều
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Start date must be before or equal to end date");

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_InvalidEnum_ShouldFail()
    {
        // Cast một số int không có trong Enum
        var query = new GetProductSalesTrendQuery(1, DateTime.UtcNow, DateTime.UtcNow, (Shared.DTOs.Reports.ReportPeriodType)999);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PeriodType);
    }
}