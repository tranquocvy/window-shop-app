using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Reports.Queries.GetSalesTrend;
using TechHaven.Application.Features.Reports.Queries.GetTopSellingProducts; // Namespace class validator của bạn
using TechHaven.Shared.Enums;
using Xunit;

namespace TechHaven.UnitTests.Features.Reports.GetSalesTrend;

[ExcludeFromCodeCoverage]
public class GetSalesTrendQueryValidatorTests
{
    private readonly GetSalesTrendQueryValidator _validator;

    public GetSalesTrendQueryValidatorTests()
    {
        _validator = new GetSalesTrendQueryValidator();
    }

    [Fact]
    public void Validate_ValidQuery_ShouldPass()
    {
        var query = new GetSalesTrendQuery(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            ReportPeriodType.Daily
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ShouldFail()
    {
        // Start Date lớn hơn End Date
        var query = new GetSalesTrendQuery(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow,
            ReportPeriodType.Daily
        );

        var result = _validator.TestValidate(query);

        // Kiểm tra lỗi logic ngày tháng
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("Start date must be before or equal to end date");

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("End date must be after or equal to start date");
    }

    [Fact]
    public void Validate_InvalidEnum_ShouldFail()
    {
        // Enum không hợp lệ
        var query = new GetSalesTrendQuery(DateTime.UtcNow, DateTime.UtcNow, (ReportPeriodType)999);

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PeriodType);
    }
}