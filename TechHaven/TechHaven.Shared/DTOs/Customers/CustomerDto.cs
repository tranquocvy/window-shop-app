namespace TechHaven.Shared.DTOs.Customers;

public class CustomerDto
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public CustomerType Type { get; set; }

    public decimal TotalPurchased { get; set; }

    public string? Note { get; set; }
}

/// <summary>
/// Defines the types of customers in the system.
/// </summary>
public enum CustomerType
{
    /// <summary>
    /// Regular customer type.
    /// </summary>
    Regular = 1,

    /// <summary>
    /// Student customer type with discounts.
    /// </summary>
    Student = 2,

    /// <summary>
    /// VIP customer type with special privileges.
    /// </summary>
    VIP = 3
}