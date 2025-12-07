namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO cho biểu đồ xu hướng bán hàng theo thời gian
/// </summary>
public class SalesTrendDto
{
  /// <summary>
  /// So sánh với kỳ trước (% tăng/giảm)
  /// </summary>
  public decimal RevenueGrowth { get; set; }
  public decimal ProfitGrowth { get; set; }

  /// <summary>
  /// Tổng cộng toàn bộ khoảng thời gian
  /// </summary>
  public SalesReportDto Summary { get; set; } = new();

  /// <summary>
  /// Danh sách điểm dữ liệu theo thời gian
  /// </summary>
  public List<SalesReportDto> DataPoints { get; set; } = new();
}

/// <summary>
/// DTO cho báo cáo doanh số bán hàng
/// </summary>
public class SalesReportDto
{
  /// <summary>
  /// Nhãn thời gian (VD: "2025-01-15", "Week 3", "Jan 2025")
  /// </summary>
  public string Period { get; set; } = string.Empty;

  /// <summary>
  /// Tổng số đơn hàng
  /// </summary>
  public int TotalOrders { get; set; }

  /// <summary>
  /// Tổng doanh thu (TotalAmount của orders)
  /// </summary>
  public decimal TotalRevenue { get; set; }

  /// <summary>
  /// Tổng chi phí (Cost = Quantity × CostPrice)
  /// </summary>
  public decimal TotalCost { get; set; }

  /// <summary>
  /// Lợi nhuận (Profit = Revenue - Cost)
  /// </summary>
  public decimal Profit { get; set; }

  /// <summary>
  /// Tỷ lệ lợi nhuận (%) = (Profit / Revenue) × 100
  /// </summary>
  public decimal ProfitMargin { get; set; }
}