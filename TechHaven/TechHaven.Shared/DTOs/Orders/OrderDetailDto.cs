namespace TechHaven.Shared.DTOs.Orders;

public class OrderDetailDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal SubTotal { get; set; }
}