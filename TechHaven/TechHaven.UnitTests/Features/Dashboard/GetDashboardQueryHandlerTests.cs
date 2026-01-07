using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Dashboard.Queries.GetDashboard;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Specifications;
using TechHaven.Shared.DTOs.Dashboard; // Namespace chứa các DTO kết quả
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Dashboard;

[ExcludeFromCodeCoverage]
public class GetDashboardQueryHandlerTests : UnitTestBase
{
    private readonly GetDashboardQueryHandler _handler;

    public GetDashboardQueryHandlerTests()
    {
        _handler = new GetDashboardQueryHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDashboardData_Correctly()
    {
        // --- ARRANGE ---
        var query = new GetDashboardQuery();
        var now = DateTime.UtcNow;

        // 1. Mock Total Products
        MockProductRepo.Setup(x => x.GetTotalProductCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(150);

        // 2. Mock Low Stock (Trả về List Product)
        var lowStockProducts = new List<Product>
        {
            new Product { ProductId = 1, ProductName = "Low Item 1", StockQuantity = 2, SellPrice = 100 },
            new Product { ProductId = 2, ProductName = "Low Item 2", StockQuantity = 0, SellPrice = 200 }
        };
        MockProductRepo.Setup(x => x.GetAsync(It.IsAny<LowStockProductsSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lowStockProducts);

        // 3. Mock Top Selling (Giả định Repo trả về list object có các field tương ứng)
        // Lưu ý: Tôi đang mock trả về TopSellingProductDto luôn để đơn giản hóa, 
        // thực tế repo của bạn có thể trả về một Projection class khác.
        // 3. Mock Top Selling
        // FIX: Thay vì dùng new TopSellingProductDto, ta dùng cú pháp Tuple (...) để khớp với Interface
        var topSelling = new List<(int ProductId, string ProductName, string? Image_Url, string BrandName, int TotalSold, decimal TotalRevenue)>
        {
            (
                ProductId: 10,
                ProductName: "Hot Item",
                Image_Url: "img_url",
                BrandName: "Brand A",
                TotalSold: 50,
                TotalRevenue: 5000m
            )
        };
        MockOrderRepo.Setup(x => x.GetTopSellingProductsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topSelling);

        // 4 & 5. Mock Today Stats
        MockOrderRepo.Setup(x => x.GetTodayOrderCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10);
        MockOrderRepo.Setup(x => x.GetTodayRevenueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(15000);

        // 6. Mock Recent Orders
        var recentOrders = new List<Order>
        {
            new Order { OrderId = 999, TotalAmount = 100, Status = Domain.Enums.OrderStatus.Completed, OrderDate = now }
        };
        MockOrderRepo.Setup(x => x.GetRecentOrdersAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentOrders);

        // 7. Mock Monthly Revenue (Dictionary Logic)
        // Tạo giả dữ liệu cho ngày mùng 1 và ngày hôm nay
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
        var revenueData = new Dictionary<DateTime, (decimal Revenue, int OrderCount)>
        {
            { firstDayOfMonth, (1000m, 5) }, // Ngày mùng 1 có doanh thu
            // Các ngày khác không có trong dict sẽ mặc định là 0
        };

        MockOrderRepo.Setup(x => x.GetMonthlyRevenueAsync(now.Year, now.Month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revenueData);

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        var data = result.Data;

        // Verify Simple Data
        data!.TotalProducts.Should().Be(150);
        data.TodayOrderCount.Should().Be(10);
        data.TodayRevenue.Should().Be(15000);

        // Verify Low Stock
        data.LowStockProducts.Should().HaveCount(2);
        data.LowStockProducts.First().ProductName.Should().Be("Low Item 1");

        // Verify Top Selling
        data.TopSellingProducts.Should().HaveCount(1);
        data.TopSellingProducts.First().TotalRevenue.Should().Be(5000);

        // Verify Recent Orders
        data.RecentOrders.Should().HaveCount(1);
        data.RecentOrders.First().OrderId.Should().Be(999);

        // Verify Monthly Logic (Quan trọng nhất)
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        data.MonthlyRevenue.Should().HaveCount(daysInMonth, "Must generate data for every day in month");

        // Check ngày mùng 1 (Có dữ liệu mock)
        var day1Data = data.MonthlyRevenue.FirstOrDefault(x => x.Date.Day == 1);
        day1Data.Should().NotBeNull();
        day1Data!.Revenue.Should().Be(1000m);
        day1Data.OrderCount.Should().Be(5);

        // Check ngày mùng 2 (Không có dữ liệu mock -> Phải bằng 0)
        if (daysInMonth >= 2)
        {
            var day2Data = data.MonthlyRevenue.FirstOrDefault(x => x.Date.Day == 2);
            day2Data!.Revenue.Should().Be(0);
            day2Data.OrderCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var query = new GetDashboardQuery();

        // Giả lập lỗi tại bước lấy TotalProducts
        MockProductRepo.Setup(x => x.GetTotalProductCountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to retrieve dashboard data");
        result.ErrorMessage.Should().Contain("Database connection failed");
    }
}