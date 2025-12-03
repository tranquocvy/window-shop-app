namespace TechHaven.Shared.DTOs.Reports;

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