namespace TechHaven.Shared.DTOs.Dashboard;

/// <summary>
/// DTO cho doanh thu theo ngày
/// </summary>
public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}