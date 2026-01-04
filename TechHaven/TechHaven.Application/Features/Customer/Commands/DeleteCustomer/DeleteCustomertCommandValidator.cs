using FluentValidation;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
  public DeleteCustomerCommandValidator()
  {
    RuleFor(x => x.CustomerId)
      .GreaterThan(0).WithMessage("Customer ID is required.");
  }
}