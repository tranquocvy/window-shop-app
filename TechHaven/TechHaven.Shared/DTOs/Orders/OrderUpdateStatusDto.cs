namespace TechHaven.Shared.DTOs.Orders;

public class OrderUpdateStatusDto
{
    public int OrderId { get; set; }

    public OrderStatus Status { get; set; }
}