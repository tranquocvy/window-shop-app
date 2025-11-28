namespace TechHaven.Shared.DTOs.Dashboard;

/// <summary>
/// DTO cho sản phẩm bán chạy
/// </summary>
public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Image_Url { get; set; } = string.Empty;   
    public string BrandName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}