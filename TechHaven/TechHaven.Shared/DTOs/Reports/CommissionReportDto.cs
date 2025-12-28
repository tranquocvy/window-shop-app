namespace TechHaven.Shared.DTOs.Reports;

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