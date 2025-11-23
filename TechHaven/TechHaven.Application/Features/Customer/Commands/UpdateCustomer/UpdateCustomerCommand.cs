using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Application.Features.Customer.Commands.UpdateCustomer;

public record UpdateCustomerCommand() : ICommand<CustomerDto>
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