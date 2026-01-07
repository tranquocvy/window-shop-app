using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Reports.Queries.GetProductsTrend;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Reports;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Reports.GetProductsTrend;

[ExcludeFromCodeCoverage]
public class GetProductSalesTrendQueryHandlerTests : UnitTestBase
{
    private readonly GetProductSalesTrendQueryHandler _handler;

    public GetProductSalesTrendQueryHandlerTests()
    {
        _handler = new GetProductSalesTrendQueryHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldAggregateTotals_Correctly()
    {
        // --- ARRANGE ---
        // Input: Lấy báo cáo cho Product Id = 1
        var query = new GetProductSalesTrendQuery(1, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, Shared.DTOs.Reports.ReportPeriodType.Daily);

        // 1. Mock Data Points (Dữ liệu chi tiết từ Repository)
        // Repo trả về List Tuple: (Period, Quantity, Revenue)
        var dataPoints = new List<(string Period, int QuantitySold, decimal Revenue)>
        {
            ("2024-01-01", 10, 1000m), // Ngày 1: Bán 10, Thu 1000
            ("2024-01-02", 5, 500m)    // Ngày 2: Bán 5, Thu 500
        };

        // 2. Mock Product Info
        var product = new Product { ProductId = 1, ProductName = "iPhone 15" };

        // Setup Mocks
        MockOrderRepo.Setup(x => x.GetProductSalesDataPointAsync(
                query.ProductId,
                query.StartDate,
                query.EndDate,
                It.IsAny<Domain.Enums.ReportPeriodType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataPoints);

        MockProductRepo.Setup(x => x.GetByIdAsync(query.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // 1. Kiểm tra Mapping thông tin sản phẩm
        result.Data!.ProductName.Should().Be("iPhone 15");

        // 2. Kiểm tra Logic Tính Tổng (Aggregation Logic) trong RAM
        // Tổng số lượng = 10 + 5 = 15
        result.Data.TotalQuantitySold.Should().Be(15);

        // Tổng doanh thu = 1000 + 500 = 1500
        result.Data.TotalRevenue.Should().Be(1500m);

        // 3. Kiểm tra danh sách chi tiết
        result.Data.DataPoints.Should().HaveCount(2);
        result.Data.DataPoints.First().Revenue.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnEmptyName_ButDataStillExists()
    {
        // --- ARRANGE ---
        // Trường hợp: Product đã bị xóa mềm hoặc lỗi data, nhưng lịch sử đơn hàng (Order) vẫn còn
        var query = new GetProductSalesTrendQuery(99, DateTime.UtcNow, DateTime.UtcNow, Shared.DTOs.Reports.ReportPeriodType.Daily);

        var dataPoints = new List<(string Period, int QuantitySold, decimal Revenue)>
        {
            ("2024-01-01", 2, 200m)
        };

        MockOrderRepo.Setup(x => x.GetProductSalesDataPointAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Domain.Enums.ReportPeriodType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataPoints);

        // Mock Product -> Null
        MockProductRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();

        // Verify logic: product?.ProductName ?? string.Empty
        result!.Data!.ProductName.Should().BeEmpty();

        // Số liệu vẫn phải hiển thị dù không thấy tên sản phẩm
        result.Data.TotalRevenue.Should().Be(200m);
    }


}