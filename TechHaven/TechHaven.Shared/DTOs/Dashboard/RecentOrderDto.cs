namespace TechHaven.Shared.DTOs.Dashboard;

/// <summary>
/// DTO cho đơn hàng gần nhất
/// </summary>
public class RecentOrderDto
{
  public int OrderId { get; set; }
  public string? CustomerName { get; set; }
  public DateTime OrderDate { get; set; }
  public decimal TotalAmount { get; set; }
  public string Status { get; set; } = string.Empty;
}