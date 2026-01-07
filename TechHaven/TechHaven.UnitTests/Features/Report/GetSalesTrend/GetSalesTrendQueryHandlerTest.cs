using FluentAssertions;
using FluentValidation.TestHelper;
using System.Diagnostics.CodeAnalysis;
using Moq;
using TechHaven.Application.Features.Reports.Queries.GetSalesTrend;
using TechHaven.Domain.Common;
using TechHaven.Shared.DTOs.Reports;
using TechHaven.Shared.Enums;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Reports.GetSalesTrend;

[ExcludeFromCodeCoverage]
public class GetSalesTrendQueryHandlerTests : UnitTestBase
{
    private readonly GetSalesTrendQueryHandler _handler;

    public GetSalesTrendQueryHandlerTests()
    {
        _handler = new GetSalesTrendQueryHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ValidData_ShouldCalculateGrowth_And_Margins_Correctly()
    {
        // --- ARRANGE ---
        // Giả lập kỳ hiện tại là 10 ngày (01/01 -> 10/01)
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 10);
        var query = new GetSalesTrendQuery(startDate, endDate, ReportPeriodType.Daily);

        // 1. Mock Data Kỳ Hiện Tại (Current Period)
        // Revenue = 200, Cost = 100 -> Profit = 100
        var currentData = new List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
        {
            ("2024-01-01", 10, 200m, 100m, 100m, 50m)
        };

        // 2. Mock Data Kỳ Trước (Previous Period)
        // Logic Handler: Previous Start = Start - (Duration) - 1
        // Revenue = 100, Cost = 50 -> Profit = 50
        var previousData = new List<(string Period, int TotalOrders, decimal TotalRevenue, decimal TotalCost, decimal Profit, decimal ProfitMargin)>
        {
            ("2023-12-21", 5, 100m, 50m, 50m, 50m)
        };

        // Setup Mock cho Kỳ Hiện Tại
        MockOrderRepo.Setup(x => x.GetSalesReportAsync(startDate, endDate, It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentData);

        // Setup Mock cho Kỳ Trước (Quan trọng: Verify Date Logic)
        // Duration = 10 ngày. Kỳ trước phải kết thúc vào ngày StartDate - 1 (31/12)
        // Unit Test này verify luôn việc Handler tính ngày đúng hay sai.
        MockOrderRepo.Setup(x => x.GetSalesReportAsync(
                It.Is<DateTime>(d => d < startDate), // Previous Start
                It.Is<DateTime>(d => d == startDate.AddDays(-1)), // Previous End
                It.IsAny<Domain.Enums.ReportPeriodType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousData);

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // 1. Verify Summary Math (Tính toán tổng)
        // Revenue = 200
        result.Data.Summary.TotalRevenue.Should().Be(200m);
        // Profit Margin = (200 - 100) / 200 = 50%
        result.Data.Summary.ProfitMargin.Should().Be(50m);

        // 2. Verify Growth Math (Tính tăng trưởng)
        // Revenue Growth = ((Current 200 - Prev 100) / Prev 100) * 100 = 100%
        result.Data.RevenueGrowth.Should().Be(100m);

        // Profit Growth = ((Current 100 - Prev 50) / Prev 50) * 100 = 100%
        result.Data.ProfitGrowth.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_PreviousRevenueIsZero_ShouldHandleDivisionByZero_ForGrowth()
    {
        // --- ARRANGE ---
        var query = new GetSalesTrendQuery(DateTime.UtcNow, DateTime.UtcNow, ReportPeriodType.Daily);

        // Kỳ hiện tại: Có doanh thu (100)
        var currentData = new List<(string, int, decimal, decimal, decimal, decimal)>
        {
            ("Today", 1, 100m, 50m, 50m, 50m)
        };

        // Kỳ trước: Doanh thu = 0
        var previousData = new List<(string, int, decimal, decimal, decimal, decimal)>(); // Empty list = Sum is 0

        MockOrderRepo.SetupSequence(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentData)  // Call 1: Current
            .ReturnsAsync(previousData); // Call 2: Previous

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        // Logic: Nếu kỳ trước = 0 và kỳ này > 0 -> Tăng trưởng 100% (Thay vì lỗi chia cho 0)
        result.Data.RevenueGrowth.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_ZeroRevenue_ShouldHandleDivisionByZero_ForProfitMargin()
    {
        // --- ARRANGE ---
        var query = new GetSalesTrendQuery(DateTime.UtcNow, DateTime.UtcNow, ReportPeriodType.Daily);

        // Kỳ hiện tại: Doanh thu = 0
        var currentData = new List<(string, int, decimal, decimal, decimal, decimal)>
        {
            ("Today", 0, 0m, 0m, 0m, 0m)
        };

        MockOrderRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentData);

        // Mock kỳ trước trả về rỗng cho gọn
        MockOrderRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<(string, int, decimal, decimal, decimal, decimal)>());

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        // Logic: Nếu Revenue = 0 -> Margin = 0 (Thay vì lỗi chia cho 0)
        result.Data.Summary.ProfitMargin.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_Exception_ShouldReturnFailure()
    {
        // --- ARRANGE ---
        var query = new GetSalesTrendQuery(DateTime.UtcNow, DateTime.UtcNow, ReportPeriodType.Daily);
        MockOrderRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database Error"));

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to get sales trend");
    }
}