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

/// <summary>
/// DTO cho báo cáo sản phẩm bán chạy
/// </summary>
public class ProductSalesDto
{
  public int ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string BrandName { get; set; } = string.Empty;

  /// <summary>
  /// Tổng số lượng đã bán
  /// </summary>
  public int QuantitySold { get; set; }

  /// <summary>
  /// Tổng doanh thu từ sản phẩm này
  /// </summary>
  public decimal TotalRevenue { get; set; }

  /// <summary>
  /// Tổng lợi nhuận từ sản phẩm này
  /// </summary>
  public decimal TotalProfit { get; set; }
}

/// <summary>
/// DTO cho báo cáo hoa hồng nhân viên
/// </summary>
public class CommissionReportDto
{
  public int UserId { get; set; }
  public string UserFullName { get; set; } = string.Empty;
  public string RoleName { get; set; } = string.Empty;

  /// <summary>
  /// Tổng doanh số bán được
  /// </summary>
  public decimal TotalSales { get; set; }

  /// <summary>
  /// Tỷ lệ hoa hồng (%)
  /// </summary>
  public decimal CommissionRate { get; set; }

  /// <summary>
  /// Số tiền hoa hồng = TotalSales × CommissionRate / 100
  /// </summary>
  public decimal CommissionAmount { get; set; }

  /// <summary>
  /// Số đơn hàng đã bán
  /// </summary>
  public int TotalOrders { get; set; }
}