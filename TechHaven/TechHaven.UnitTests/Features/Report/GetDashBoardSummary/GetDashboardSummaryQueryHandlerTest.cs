using FluentAssertions;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TechHaven.Application.Features.Reports.Queries.GetDashboardSummary;
using TechHaven.Domain.Common;
using TechHaven.Domain.Entities;
using TechHaven.Shared.DTOs.Reports;
using TechHaven.UnitTests.Common;
using Xunit;

namespace TechHaven.UnitTests.Features.Reports.GetDashboardSummary;

[ExcludeFromCodeCoverage]
public class GetDashboardSummaryQueryHandlerTests : UnitTestBase
{
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _handler = new GetDashboardSummaryQueryHandler(MockUow.Object);
    }

    [Fact]
    public async Task Handle_ShouldCalculateStatsAndProfit_Correctly()
    {
        // --- ARRANGE ---
        var query = new GetDashboardSummaryQuery();

        // 1. Mock Data for Orders (Today & Month) để test hàm CalculateProfit
        // Logic: Revenue = TotalAmount. Cost = Quantity * Product.CostPrice.
        var productA = new Product { ProductId = 1, CostPrice = 50, ProductName = "Prod A" };

        var order1 = new Order
        {
            TotalAmount = 100, // Revenue
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { Quantity = 1, Product = productA } // Cost = 1 * 50 = 50
            }
            // Profit Order 1 = 100 - 50 = 50
        };

        var todayOrders = new List<Order> { order1 }; // Total Profit = 50
        var monthOrders = new List<Order> { order1, order1 }; // Total Profit = 100

        MockOrderRepo.Setup(x => x.GetTodayOrders(It.IsAny<CancellationToken>())).ReturnsAsync(todayOrders);
        MockOrderRepo.Setup(x => x.GetMonthOrders(It.IsAny<CancellationToken>())).ReturnsAsync(monthOrders);

        // 2. Mock Inventory
        var lowStock = new List<Product> { new Product(), new Product() }; // Count = 2
        var outStock = new List<Product> { new Product() }; // Count = 1

        MockProductRepo.Setup(x => x.GetLowStockAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(lowStock);
        MockProductRepo.Setup(x => x.GetOutOfStockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(outStock);
        MockProductRepo.Setup(x => x.GetTotalProductCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(99);

        // 3. Mock Top Selling Products (Tuple return type như đã thống nhất)
        var topSellingTuples = new List<(int ProductId, string ProductName, string? Image_Url, string BrandName, int TotalSold, decimal TotalRevenue)>
        {
            (1, "Prod A", "img", "Brand", 10, 1000m)
        };
        MockOrderRepo.Setup(x => x.GetTopSellingProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topSellingTuples);

        // 4. Mock Commission Report (Top Sellers) Moq ko nhận kiểu tuple
        //var topSellers = new List<CommissionReportDto>
        //{
        //    new CommissionReportDto { UserFullName = "Seller One", TotalSales = 500 }
        //};
        var topSellers = new List<(int UserId, string UserFullName, string RoleName, decimal TotalSales, decimal CommissionRate, decimal CommissionAmount, int TotalOrders)>
        {
            (
                UserId: 1,
                UserFullName: "Seller One",
                RoleName: "Seller",
                TotalSales: 500m,
                CommissionRate: 5.0m,
                CommissionAmount: 25m,
                TotalOrders: 10
            )
        };
        MockOrderRepo.Setup(x => x.GetCommissionReportAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(topSellers);

        // 5. Mock Order Status Counts
        var statusCounts = new Dictionary<Domain.Enums.OrderStatus, int>
        {
            { Domain.Enums.OrderStatus.Pending, 5 },
            { Domain.Enums.OrderStatus.Processing, 3 },
            { Domain.Enums.OrderStatus.Completed, 10 },
            { Domain.Enums.OrderStatus.Cancelled, 2 }
        };
        MockOrderRepo.Setup(x => x.GetOrderStatusCountsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusCounts);

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeTrue();
        var data = result.Data;

        // Verify Profit Logic (Quan trọng)
        data!.TodayRevenue.Should().Be(100);
        data.TodayProfit.Should().Be(50); // 100 (Rev) - 50 (Cost)

        data.MonthRevenue.Should().Be(200);
        data.MonthProfit.Should().Be(100); // 200 (Rev) - 100 (Cost)

        // Verify Counts
        data.LowStockProducts.Should().Be(2);
        data.OutOfStockProducts.Should().Be(1);
        data.TotalProducts.Should().Be(99);

        // Verify Status
        data.PendingOrders.Should().Be(5);
        data.CompletedOrders.Should().Be(10);

        // Verify Lists
        data.TopSellingProducts.Should().HaveCount(1);
        data.TopSellers.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Exception_ShouldReturnInternalError()
    {
        // --- ARRANGE ---
        var query = new GetDashboardSummaryQuery();
        MockOrderRepo.Setup(x => x.GetTodayOrders(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Ouch"));

        // --- ACT ---
        var result = await _handler.Handle(query, CancellationToken.None);

        // --- ASSERT ---
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorType.InternalError);
        result.ErrorMessage.Should().Contain("Failed to get dashboard summary");
    }
}