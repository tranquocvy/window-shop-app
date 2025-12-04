namespace TechHaven.Shared.DTOs.Reports;

/// <summary>
/// DTO cho query báo cáo với bộ lọc thời gian
/// </summary>
public class ReportQueryDto
{
  /// <summary>
  /// Ngày bắt đầu (bắt buộc)
  /// </summary>
  public DateTime StartDate { get; set; }

  /// <summary>
  /// Ngày kết thúc (bắt buộc)
  /// </summary>
  public DateTime EndDate { get; set; }

  /// <summary>
  /// Loại báo cáo: Daily, Weekly, Monthly, Yearly
  /// </summary>
  public ReportPeriodType PeriodType { get; set; } = ReportPeriodType.Daily;

  public  int? UserId { get; set; }    

}

/// <summary>
/// Loại khoảng thời gian báo cáo
/// </summary>
public enum ReportPeriodType
{
  Daily = 1,
  Weekly = 2,
  Monthly = 3,
  Yearly = 4
}