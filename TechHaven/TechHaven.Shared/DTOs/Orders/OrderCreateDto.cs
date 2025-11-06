namespace TechHaven.Shared.DTOs.Orders;

public class OrderCreateDto
{
    public int? CustomerId { get; set; }

    public decimal Discount { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<OrderCreateItemDto> Items { get; set; } = Array.Empty<OrderCreateItemDto>();
}

public class OrderCreateItemDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}