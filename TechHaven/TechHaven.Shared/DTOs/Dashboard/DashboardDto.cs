namespace TechHaven.Shared.DTOs.Dashboard;

/// <summary>
/// Dashboard overview DTO containing all summary information
/// </summary>
public class DashboardDto
{
    /// <summary>
    /// Tổng số sản phẩm trong hệ thống
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Tổng số đơn hàng trong hệ thống
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Top 5 sản phẩm sắp hết hàng (số lượng < 5)
    /// </summary>
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();

    /// <summary>
    /// Top 5 sản phẩm bán chạy nhất
    /// </summary>
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

    /// <summary>
    /// Tổng số đơn hàng trong ngày hôm nay
    /// </summary>
    public int TodayOrderCount { get; set; }

    /// <summary>
    /// Tổng doanh thu trong ngày hôm nay
    /// </summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>
    /// 3 đơn hàng gần nhất
    /// </summary>
    public List<RecentOrderDto> RecentOrders { get; set; } = new();

    /// <summary>
    /// Doanh thu theo ngày trong tháng hiện tại
    /// </summary>
    public List<DailyRevenueDto> MonthlyRevenue { get; set; } = new();
}