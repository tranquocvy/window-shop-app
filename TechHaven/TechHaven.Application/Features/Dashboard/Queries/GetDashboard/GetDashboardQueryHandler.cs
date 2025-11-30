using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.Specifications;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, Result<DashboardDto>>
{
  private readonly IUnitOfWork _unitOfWork;

  public GetDashboardQueryHandler(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result<DashboardDto>> Handle(
      GetDashboardQuery request,
      CancellationToken cancellationToken)
  {
    try
    {
      // 1. Tổng số sản phẩm
      var totalProducts = await _unitOfWork.Products
          .GetTotalProductCountAsync(cancellationToken);

      // 2. Top 5 sản phẩm sắp hết hàng (stock < 5)
      var lowStockSpec = new LowStockProductsSpecification(threshold: 5);
      var lowStockProducts = await _unitOfWork.Products
          .GetAsync(lowStockSpec, cancellationToken);

      var lowStockDtos = lowStockProducts
          .Take(5)
          .Select(p => new LowStockProductDto
          {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Image_Url = p.ImageUrl ?? string.Empty,
            BrandName = p.BrandName,
            StockQuantity = p.StockQuantity,
            SellPrice = p.SellPrice
          })
          .ToList();

      // 3. Top 5 sản phẩm bán chạy
      var topSelling = await _unitOfWork.Orders
          .GetTopSellingProductsAsync(5, cancellationToken);

      var topSellingDtos = topSelling
          .Select(x => new TopSellingProductDto
          {
            ProductId = x.ProductId,
            ProductName = x.ProductName,
            Image_Url = x.Image_Url,
            BrandName = x.BrandName,
            TotalSold = x.TotalSold,
            TotalRevenue = x.TotalRevenue
          })
          .ToList();

      // 4. Tổng đơn hàng trong ngày
      var todayOrderCount = await _unitOfWork.Orders
          .GetTodayOrderCountAsync(cancellationToken);

      // 5. Tổng doanh thu trong ngày
      var todayRevenue = await _unitOfWork.Orders
          .GetTodayRevenueAsync(cancellationToken);

      // 6. 3 đơn hàng gần nhất
      var recentOrders = await _unitOfWork.Orders
          .GetRecentOrdersAsync(3, cancellationToken);

      var recentOrderDtos = recentOrders
          .Select(o => new RecentOrderDto
          {
            OrderId = o.OrderId,
            CustomerName = o.Customer?.CustomerName ?? "Walk-in",
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString()
          })
          .ToList();

      // 7. Doanh thu theo ngày trong tháng hiện tại
      var now = DateTime.Now;
      var monthlyRevenue = await _unitOfWork.Orders
          .GetMonthlyRevenueAsync(now.Year, now.Month, cancellationToken);

      // Tạo list đầy đủ các ngày trong tháng
      var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
      var dailyRevenueDtos = new List<DailyRevenueDto>();

      for (int day = 1; day <= daysInMonth; day++)
      {
        var date = new DateTime(now.Year, now.Month, day);
        var hasData = monthlyRevenue.TryGetValue(date, out var data);

        dailyRevenueDtos.Add(new DailyRevenueDto
        {
          Date = date,
          Revenue = hasData ? data.Revenue : 0,
          OrderCount = hasData ? data.OrderCount : 0
        });
      }

      // Tạo dashboard DTO
      var dashboard = new DashboardDto
      {
        TotalProducts = totalProducts,
        LowStockProducts = lowStockDtos,
        TopSellingProducts = topSellingDtos,
        TodayOrderCount = todayOrderCount,
        TodayRevenue = todayRevenue,
        RecentOrders = recentOrderDtos,
        MonthlyRevenue = dailyRevenueDtos
      };

      return Result<DashboardDto>.Success(dashboard);
    }
    catch (Exception ex)
    {
      return Result<DashboardDto>.Failure(
          $"Failed to retrieve dashboard data: {ex.Message}",
          ErrorType.InternalError);
    }
  }
}