namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO cho query báo cáo hoa hồng của nhân viên theo thời gian (tháng + năm)
/// </summary>
public class CommissionQueryDto
{
  /// <summary>
  /// Tháng (bắt buộc)
  /// </summary>
  public int Month { get; set; }

  /// <summary>
  /// Năm (bắt buộc)
  /// </summary>
  public int Year { get; set; }
}