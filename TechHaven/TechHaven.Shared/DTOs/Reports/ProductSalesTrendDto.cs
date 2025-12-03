using System.Text.Json.Serialization;

namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO báo cáo xu hướng bán hàng cho một sản phẩm cụ thể
/// </summary>
public class ProductSalesTrendDto
{
  public int ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;

  /// <summary>
  /// Tổng số lượng đã bán trong khoảng thời gian chọn
  /// </summary>
  public int TotalQuantitySold { get; set; }

  /// <summary>
  /// Tổng doanh thu trong khoảng thời gian chọn
  /// </summary>
  public decimal TotalRevenue { get; set; }

  /// <summary>
  /// Danh sách điểm dữ liệu để vẽ biểu đồ (bao gồm cả những ngày doanh thu = 0)
  /// </summary>
  public List<ProductSalesDataPointDto> DataPoints { get; set; } = new();
}

public class ProductSalesDataPointDto
{
  /// <summary>
  /// Trục hoành: Thời gian (VD: "2023-10-01")
  /// </summary>
  public string Period { get; set; } = string.Empty;

  /// <summary>
  /// Trục tung: Số lượng bán (Dữ liệu chính để vẽ biểu đồ)
  /// </summary>
  public int QuantitySold { get; set; }

  /// <summary>
  /// Tooltip: Doanh thu tại thời điểm đó
  /// </summary>
  public decimal Revenue { get; set; }
}