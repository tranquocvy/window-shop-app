using TechHaven.Application.Interfaces;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : ICommand;