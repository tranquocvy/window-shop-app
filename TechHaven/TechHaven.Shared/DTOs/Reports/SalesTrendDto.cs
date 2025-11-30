namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO cho biểu đồ xu hướng bán hàng theo thời gian
/// </summary>
public class SalesTrendDto
{
  /// <summary>
  /// Danh sách điểm dữ liệu theo thời gian
  /// </summary>
  public List<SalesReportDto> DataPoints { get; set; } = new();

  /// <summary>
  /// Tổng cộng toàn bộ khoảng thời gian
  /// </summary>
  public SalesReportDto Summary { get; set; } = new();

  /// <summary>
  /// So sánh với kỳ trước (% tăng/giảm)
  /// </summary>
  public decimal RevenueGrowth { get; set; }
  public decimal ProfitGrowth { get; set; }
}