namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO tổng hợp cho Dashboard - hiển thị overview
/// </summary>
public class DashboardSummaryDto
{
  // ===== TODAY STATISTICS =====
  public decimal TodayRevenue { get; set; }
  public decimal TodayProfit { get; set; }
  public int TodayOrders { get; set; }

  // ===== THIS MONTH STATISTICS =====
  public decimal MonthRevenue { get; set; }
  public decimal MonthProfit { get; set; }
  public int MonthOrders { get; set; }

  // ===== INVENTORY STATUS =====
  public int TotalProducts { get; set; }
  public int LowStockProducts { get; set; }
  public int OutOfStockProducts { get; set; }

  // ===== TOP PERFORMERS =====
  /// <summary>
  /// Top 5 sản phẩm bán chạy nhất
  /// </summary>
  public List<ProductSalesDto> TopSellingProducts { get; set; } = new();

  /// <summary>
  /// Top 5 nhân viên bán hàng xuất sắc
  /// </summary>
  public List<CommissionReportDto> TopSellers { get; set; } = new();

  // ===== ORDER STATUS BREAKDOWN =====
  public int PendingOrders { get; set; }
  public int ProcessingOrders { get; set; }
  public int CompletedOrders { get; set; }
  public int CancelledOrders { get; set; }
}