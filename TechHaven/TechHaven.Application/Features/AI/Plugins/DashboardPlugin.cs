using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.AI.Plugins;

/// <summary>
/// Plugin cung cấp các function để AI phân tích dashboard và thống kê
/// </summary>
public class DashboardPlugin
{
  private readonly IUnitOfWork _unitOfWork;

  public DashboardPlugin(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  [KernelFunction("get_dashboard_summary")]
  [Description("Lấy tổng quan dashboard hôm nay và tháng này")]
  public async Task<string> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
  {
    var today = DateTime.UtcNow.Date;
    var startOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    // Today stats
    var todayOrders = await _unitOfWork.Orders.GetTodayOrders();
    var todayRevenue = todayOrders.Sum(o => o.TotalAmount);
    var todayProfit = CalculateProfit(todayOrders.ToList());

    // Month stats
    var monthOrders = await _unitOfWork.Orders.GetMonthOrders();
    var monthRevenue = monthOrders.Sum(o => o.TotalAmount);
    var monthProfit = CalculateProfit(monthOrders.ToList());

    // Inventory
    var lowStockProducts = await _unitOfWork.Products.GetLowStockAsync();
    var outOfStockProducts = await _unitOfWork.Products.GetOutOfStockAsync();
    var totalProducts = await _unitOfWork.Products.GetTotalProductCountAsync();

    var sb = new StringBuilder();
    sb.AppendLine("📊 TỔNG QUAN DASHBOARD");
    sb.AppendLine();
    sb.AppendLine("📅 HÔM NAY:");
    sb.AppendLine($"• Doanh thu: {todayRevenue:N0} VND");
    sb.AppendLine($"• Lợi nhuận: {todayProfit:N0} VND");
    sb.AppendLine($"• Số đơn hàng: {todayOrders.Count()}");
    sb.AppendLine();
    sb.AppendLine("📆 THÁNG NÀY:");
    sb.AppendLine($"• Doanh thu: {monthRevenue:N0} VND");
    sb.AppendLine($"• Lợi nhuận: {monthProfit:N0} VND");
    sb.AppendLine($"• Số đơn hàng: {monthOrders.Count()}");
    sb.AppendLine();
    sb.AppendLine("📦 KHO HÀNG:");
    sb.AppendLine($"• Tổng sản phẩm: {totalProducts}");
    sb.AppendLine($"• Sắp hết hàng: {lowStockProducts.Count()}");
    sb.AppendLine($"• Hết hàng: {outOfStockProducts.Count()}");

    return sb.ToString();
  }

  [KernelFunction("analyze_sales_trend")]
  [Description("Phân tích xu hướng bán hàng trong khoảng thời gian")]
  public async Task<string> AnalyzeSalesTrendAsync(
      [Description("Số ngày để phân tích (mặc định 7 ngày)")] int days = 7,
      CancellationToken cancellationToken = default)
  {
    var endDate = DateTime.UtcNow.Date;
    var startDate = endDate.AddDays(-days);

    var report = await _unitOfWork.Orders.GetSalesReportAsync(
        startDate,
        endDate,
        Domain.Enums.ReportPeriodType.Daily,
        cancellationToken);

    if (!report.Any())
    {
      return $"Không có dữ liệu bán hàng trong {days} ngày qua.";
    }

    var totalRevenue = report.Sum(r => r.TotalRevenue);
    var totalOrders = report.Sum(r => r.TotalOrders);
    var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
    var totalProfit = report.Sum(r => r.Profit);
    var avgProfitMargin = report.Average(r => r.ProfitMargin);

    // Tìm ngày bán tốt nhất
    var bestDay = report.OrderByDescending(r => r.TotalRevenue).First();

    var sb = new StringBuilder();
    sb.AppendLine($"📈 PHÂN TÍCH {days} NGÀY QUA ({startDate:dd/MM} - {endDate:dd/MM})");
    sb.AppendLine();
    sb.AppendLine("💰 TỔNG QUAN:");
    sb.AppendLine($"• Tổng doanh thu: {totalRevenue:N0} VND");
    sb.AppendLine($"• Tổng lợi nhuận: {totalProfit:N0} VND");
    sb.AppendLine($"• Số đơn hàng: {totalOrders}");
    sb.AppendLine($"• Giá trị TB/đơn: {avgOrderValue:N0} VND");
    sb.AppendLine($"• Tỷ lệ lợi nhuận TB: {avgProfitMargin:F1}%");
    sb.AppendLine();
    sb.AppendLine("🌟 NGÀY BÁN CHẠY NHẤT:");
    sb.AppendLine($"• Ngày: {bestDay.Period}");
    sb.AppendLine($"• Doanh thu: {bestDay.TotalRevenue:N0} VND");
    sb.AppendLine($"• Số đơn: {bestDay.TotalOrders}");

    return sb.ToString();
  }

  [KernelFunction("get_top_products")]
  [Description("Lấy danh sách sản phẩm bán chạy nhất")]
  public async Task<string> GetTopProductsAsync(
      [Description("Số ngày để thống kê (mặc định 30)")] int days = 30,
      [Description("Số lượng sản phẩm hiển thị (mặc định 5)")] int count = 5,
      CancellationToken cancellationToken = default)
  {
    var endDate = DateTime.UtcNow.Date;
    var startDate = endDate.AddDays(-days);

    var topProducts = await _unitOfWork.Orders.GetTopSellingProductsAsync(
        startDate,
        endDate,
        count,
        cancellationToken);

    if (!topProducts.Any())
    {
      return $"Không có dữ liệu sản phẩm trong {days} ngày qua.";
    }

    var sb = new StringBuilder();
    sb.AppendLine($"🏆 TOP {count} SẢN PHẨM BÁN CHẠY ({days} ngày qua)");
    sb.AppendLine();

    int rank = 1;
    foreach (var product in topProducts)
    {
      sb.AppendLine($"{rank}. {product.ProductName} ({product.BrandName})");
      sb.AppendLine($"   • Đã bán: {product.TotalSold} sản phẩm");
      sb.AppendLine($"   • Doanh thu: {product.TotalRevenue:N0} VND");
      sb.AppendLine();
      rank++;
    }

    return sb.ToString();
  }

  [KernelFunction("get_low_stock_alert")]
  [Description("Cảnh báo sản phẩm sắp hết hàng hoặc hết hàng")]
  public async Task<string> GetLowStockAlertAsync(
      [Description("Ngưỡng cảnh báo (mặc định 5)")] int threshold = 5,
      CancellationToken cancellationToken = default)
  {
    var lowStock = await _unitOfWork.Products.GetLowStockAsync(threshold);
    var outOfStock = await _unitOfWork.Products.GetOutOfStockAsync();

    var sb = new StringBuilder();
    sb.AppendLine("⚠️ CẢNH BÁO KHO HÀNG");
    sb.AppendLine();

    if (outOfStock.Any())
    {
      sb.AppendLine($"🔴 HẾT HÀNG ({outOfStock.Count()} sản phẩm):");
      foreach (var product in outOfStock.Take(5))
      {
        sb.AppendLine($"• {product.ProductName} ({product.BrandName})");
      }
      if (outOfStock.Count() > 5)
      {
        sb.AppendLine($"... và {outOfStock.Count() - 5} sản phẩm khác");
      }
      sb.AppendLine();
    }

    if (lowStock.Any())
    {
      sb.AppendLine($"🟡 SẮP HẾT ({lowStock.Count()} sản phẩm, còn ≤ {threshold}):");
      foreach (var product in lowStock.Take(5))
      {
        sb.AppendLine($"• {product.ProductName}: còn {product.StockQuantity}");
      }
      if (lowStock.Count() > 5)
      {
        sb.AppendLine($"... và {lowStock.Count() - 5} sản phẩm khác");
      }
    }

    if (!outOfStock.Any() && !lowStock.Any())
    {
      sb.AppendLine("✅ Tất cả sản phẩm đều còn hàng đầy đủ!");
    }

    return sb.ToString();
  }

  [KernelFunction("compare_performance")]
  [Description("So sánh hiệu suất kinh doanh giữa 2 khoảng thời gian")]
  public async Task<string> ComparePerformanceAsync(
      [Description("Số ngày kỳ hiện tại")] int currentPeriodDays = 7,
      CancellationToken cancellationToken = default)
  {
    var currentEnd = DateTime.UtcNow.Date;
    var currentStart = currentEnd.AddDays(-currentPeriodDays);

    var previousEnd = currentStart.AddDays(-1);
    var previousStart = previousEnd.AddDays(-currentPeriodDays);

    var currentReport = await _unitOfWork.Orders.GetSalesReportAsync(
        currentStart, currentEnd, Domain.Enums.ReportPeriodType.Daily, cancellationToken);

    var previousReport = await _unitOfWork.Orders.GetSalesReportAsync(
        previousStart, previousEnd, Domain.Enums.ReportPeriodType.Daily, cancellationToken);

    var currentRevenue = currentReport.Sum(r => r.TotalRevenue);
    var previousRevenue = previousReport.Sum(r => r.TotalRevenue);
    var revenueGrowth = previousRevenue > 0
        ? ((currentRevenue - previousRevenue) / previousRevenue) * 100
        : 0;

    var currentOrders = currentReport.Sum(r => r.TotalOrders);
    var previousOrders = previousReport.Sum(r => r.TotalOrders);
    var ordersGrowth = previousOrders > 0
        ? ((currentOrders - previousOrders) / (decimal)previousOrders) * 100
        : 0;

    var currentProfit = currentReport.Sum(r => r.Profit);
    var previousProfit = previousReport.Sum(r => r.Profit);
    var profitGrowth = previousProfit > 0
        ? ((currentProfit - previousProfit) / previousProfit) * 100
        : 0;

    var sb = new StringBuilder();
    sb.AppendLine($"📊 SO SÁNH HIỆU SUẤT ({currentPeriodDays} ngày)");
    sb.AppendLine();
    sb.AppendLine($"Kỳ hiện tại: {currentStart:dd/MM} - {currentEnd:dd/MM}");
    sb.AppendLine($"Kỳ trước: {previousStart:dd/MM} - {previousEnd:dd/MM}");
    sb.AppendLine();

    sb.AppendLine("💰 DOANH THU:");
    sb.AppendLine($"• Hiện tại: {currentRevenue:N0} VND");
    sb.AppendLine($"• Trước đó: {previousRevenue:N0} VND");
    sb.AppendLine($"• Tăng trưởng: {GetGrowthIcon(revenueGrowth)} {revenueGrowth:F1}%");
    sb.AppendLine();

    sb.AppendLine("📦 ĐỚN HÀNG:");
    sb.AppendLine($"• Hiện tại: {currentOrders}");
    sb.AppendLine($"• Trước đó: {previousOrders}");
    sb.AppendLine($"• Tăng trưởng: {GetGrowthIcon(ordersGrowth)} {ordersGrowth:F1}%");
    sb.AppendLine();

    sb.AppendLine("💵 LỢI NHUẬN:");
    sb.AppendLine($"• Hiện tại: {currentProfit:N0} VND");
    sb.AppendLine($"• Trước đó: {previousProfit:N0} VND");
    sb.AppendLine($"• Tăng trưởng: {GetGrowthIcon(profitGrowth)} {profitGrowth:F1}%");

    return sb.ToString();
  }

  [KernelFunction("get_employee_performance")]
  [Description("Xem hiệu suất bán hàng của nhân viên trong tháng")]
  public async Task<string> GetEmployeePerformanceAsync(
      [Description("Tháng (1-12, mặc định tháng hiện tại)")] int? month = null,
      [Description("Năm (mặc định năm hiện tại)")] int? year = null,
      CancellationToken cancellationToken = default)
  {
    var now = DateTime.UtcNow;
    var targetMonth = month ?? now.Month;
    var targetYear = year ?? now.Year;

    var commissions = await _unitOfWork.Orders.GetCommissionReportAsync(
        targetMonth, targetYear, cancellationToken);

    if (!commissions.Any())
    {
      return $"Không có dữ liệu bán hàng tháng {targetMonth}/{targetYear}.";
    }

    var totalSales = commissions.Sum(c => c.TotalSales);
    var totalCommission = commissions.Sum(c => c.CommissionAmount);

    var sb = new StringBuilder();
    sb.AppendLine($"👥 HIỆU SUẤT NHÂN VIÊN - THÁNG {targetMonth}/{targetYear}");
    sb.AppendLine();
    sb.AppendLine($"Tổng doanh số: {totalSales:N0} VND");
    sb.AppendLine($"Tổng hoa hồng: {totalCommission:N0} VND");
    sb.AppendLine();
    sb.AppendLine("🏆 BẢNG XẾP HẠNG:");

    int rank = 1;
    foreach (var emp in commissions)
    {
      sb.AppendLine($"{rank}. {emp.UserFullName} ({emp.RoleName})");
      sb.AppendLine($"   • Doanh số: {emp.TotalSales:N0} VND");
      sb.AppendLine($"   • Đơn hàng: {emp.TotalOrders}");
      sb.AppendLine($"   • Hoa hồng: {emp.CommissionAmount:N0} VND ({emp.CommissionRate}%)");
      sb.AppendLine();
      rank++;
    }

    return sb.ToString();
  }

  [KernelFunction("predict_restock_needs")]
  [Description("Dự đoán nhu cầu nhập hàng dựa trên tốc độ bán")]
  public async Task<string> PredictRestockNeedsAsync(
      [Description("Số ngày để tính tốc độ bán")] int analysisDays = 30,
      CancellationToken cancellationToken = default)
  {
    var endDate = DateTime.UtcNow.Date;
    var startDate = endDate.AddDays(-analysisDays);

    // Lấy sản phẩm có tồn kho thấp
    var lowStockProducts = await _unitOfWork.Products.GetLowStockAsync(10);

    if (!lowStockProducts.Any())
    {
      return "✅ Không có sản phẩm nào cần nhập hàng khẩn cấp.";
    }

    var sb = new StringBuilder();
    sb.AppendLine($"📊 DỰ ĐOÁN NHU CẦU NHẬP HÀNG ({analysisDays} ngày qua)");
    sb.AppendLine();

    foreach (var product in lowStockProducts.Take(10))
    {
      // Tính tốc độ bán trung bình
      var spec = new Domain.Specifications.OrdersByProductAndDateRangeSpecification(
          product.ProductId, startDate, endDate);
      var orders = await _unitOfWork.Orders.GetAsync(spec, cancellationToken);

      var totalSold = orders
          .SelectMany(o => o.OrderDetails!)
          .Where(od => od.ProductId == product.ProductId)
          .Sum(od => od.Quantity);

      var avgDailySales = totalSold / (decimal)analysisDays;
      var daysUntilOutOfStock = avgDailySales > 0
          ? product.StockQuantity / avgDailySales
          : 999;

      var urgency = daysUntilOutOfStock switch
      {
        <= 3 => "🔴 KHẨN CẤP",
        <= 7 => "🟡 CẦN SỚM",
        <= 14 => "🟢 BÌN THƯỜNG",
        _ => "⚪ KHÔNG CẦN"
      };

      sb.AppendLine($"{urgency} {product.ProductName}");
      sb.AppendLine($"• Tồn kho: {product.StockQuantity}");
      sb.AppendLine($"• Bán TB: {avgDailySales:F1} sản phẩm/ngày");
      sb.AppendLine($"• Dự kiến hết sau: {daysUntilOutOfStock:F0} ngày");
      sb.AppendLine();
    }

    return sb.ToString();
  }

  // Helper methods
  private decimal CalculateProfit(List<Domain.Entities.Order> orders)
  {
    var revenue = orders.Sum(o => o.TotalAmount);
    var cost = orders
        .SelectMany(o => o.OrderDetails!)
        .Sum(od => od.Quantity * (od.Product?.CostPrice ?? 0));
    return revenue - cost;
  }

  private string GetGrowthIcon(decimal growth)
  {
    return growth > 0 ? "📈" : growth < 0 ? "📉" : "➡️";
  }
}