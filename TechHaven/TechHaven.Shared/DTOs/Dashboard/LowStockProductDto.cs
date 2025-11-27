namespace TechHaven.Shared.DTOs.Dashboard;

/// <summary>
/// DTO cho sản phẩm sắp hết hàng
/// </summary>
public class LowStockProductDto
{
  public int ProductId { get; set; }
  public string ProductName { get; set; } = string.Empty;
  public string BrandName { get; set; } = string.Empty;
  public int StockQuantity { get; set; }
  public decimal SellPrice { get; set; }
}