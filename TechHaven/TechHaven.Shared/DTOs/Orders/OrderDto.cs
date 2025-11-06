namespace TechHaven.Shared.DTOs.Orders;

public class OrderDto
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public int UserId { get; set; }

    public string? UserFullName { get; set; }

    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<OrderDetailDto> Details { get; set; } = Array.Empty<OrderDetailDto>();
}

/// <summary>
/// Defines the possible statuses of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is pending and awaiting processing.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Order is currently being processed.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Order has been completed successfully.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Order has been returned.
    /// </summary>
    Returned = 5
}