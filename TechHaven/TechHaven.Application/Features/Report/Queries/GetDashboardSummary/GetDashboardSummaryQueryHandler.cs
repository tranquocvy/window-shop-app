using TechHaven.Application.Features.Dashboard.Queries.GetDashboard;
using TechHaven.Application.Features.Reports.Queries.GetDashboardSummary;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Shared.DTOs.Reports;

public class GetDashboardSummaryQueryHandler
  : IQueryHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<DashboardSummaryDto>> Handle(
    GetDashboardSummaryQuery request,
    CancellationToken cancellationToken
  )
  {
    try
    {
      var today = DateTime.UtcNow.Date;
      var startOfMonth = new DateTime(today.Year, today.Month, 1);

      // Today stats
      var todayOrders = await _unitOfWork.Orders.GetTodayOrders();

      // Month stats
      var monthOrders = await _unitOfWork.Orders.GetMonthOrders();

      // Inventory
      var lowStockProducts = await _unitOfWork.Products.GetLowStockAsync();
      int totalLowStockProducts = lowStockProducts.Count();
      var outOfStock = await _unitOfWork.Products.GetOutOfStockAsync();
      int totalOutOfStock = outOfStock.Count();

      var topProducts = await _unitOfWork.Orders.GetTopSellingProductsAsync(
        startOfMonth,
        today,
        5,
        cancellationToken
      );

      var topProductDtos = topProducts.Select(p => new ProductSalesDto
      {
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        BrandName = p.BrandName,
        QuantitySold = p.TotalSold,
        TotalRevenue = p.TotalRevenue,
        TotalProfit = p.TotalRevenue - p.TotalSold
      }).ToList();

      var topSellers = await _unitOfWork.Orders.GetCommissionReportAsync(
        today.Year,
        today.Month,
        cancellationToken
      );

      var topSellerDtos = topSellers.Select(p => new CommissionReportDto
      {
        UserId = p.UserId,
        UserFullName = p.UserFullName,
        RoleName = p.RoleName,
        TotalSales = p.TotalSales,
        CommissionRate = p.CommissionRate,
        CommissionAmount = p.CommissionAmount,
        TotalOrders = p.TotalOrders,
      }).ToList();

      var ordersStatusDict = await _unitOfWork.Orders.GetOrderStatusCountsAsync(startOfMonth, today, cancellationToken);

      var result = new DashboardSummaryDto
      {
        TodayRevenue = todayOrders.Sum(o => o.TotalAmount),
        TodayProfit = CalculateProfit((List<TechHaven.Domain.Entities.Order>)todayOrders),
        TodayOrders = todayOrders.Count(),

        MonthRevenue = monthOrders.Sum(o => o.TotalAmount),
        MonthProfit = CalculateProfit((List<TechHaven.Domain.Entities.Order>)monthOrders),
        MonthOrders = monthOrders.Count(),

        TotalProducts = await _unitOfWork.Products.GetTotalProductCountAsync(),
        LowStockProducts = totalLowStockProducts,
        OutOfStockProducts = totalOutOfStock,

        TopSellingProducts = topProductDtos,
        TopSellers = topSellerDtos,

        PendingOrders = ordersStatusDict[TechHaven.Domain.Enums.OrderStatus.Pending],
        ProcessingOrders = ordersStatusDict[TechHaven.Domain.Enums.OrderStatus.Processing],
        CompletedOrders = ordersStatusDict[TechHaven.Domain.Enums.OrderStatus.Completed],
        CancelledOrders = ordersStatusDict[TechHaven.Domain.Enums.OrderStatus.Cancelled],
      };

      return Result<DashboardSummaryDto>.Success(result);
    }
    catch (Exception ex)
    {
      return Result<DashboardSummaryDto>.Failure(
        $"Failed to get dashboard summary for report: {ex.Message}",
        ErrorType.InternalError
      );
    }
  }

  private decimal CalculateProfit(List<TechHaven.Domain.Entities.Order> orders)
  {
    var revenue = orders.Sum(o => o.TotalAmount);
    var cost = orders
        .SelectMany(o => o.OrderDetails!)
        .Sum(od => od.Quantity * (od.Product?.CostPrice ?? 0));
    return revenue - cost;
  }
}