namespace TechHaven.Domain.Enums;

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