using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Orders;

public class OrderUpsertRequestDto
{
    public int? CustomerId { get; set; }

    public OrderStatus Status { get; set; }

    public decimal Discount { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<OrderUpsertItemDto> Items { get; set; } = Array.Empty<OrderUpsertItemDto>();
}

public class OrderUpsertItemDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}