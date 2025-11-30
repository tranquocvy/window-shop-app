namespace TechHaven.Shared.DTOs.Reports;

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