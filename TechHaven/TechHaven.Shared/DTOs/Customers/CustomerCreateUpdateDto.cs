namespace TechHaven.Shared.DTOs.Customers;

public class CustomerCreateUpdateDto
{
    public string CustomerName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public CustomerType Type { get; set; } = CustomerType.Regular;

    public string? Note { get; set; }
}