using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : ICommand<Result>;