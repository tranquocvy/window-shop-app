using FluentValidation;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteCustomerCommandValidator(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;

    RuleFor(x => x.CustomerId)
      .GreaterThan(0).WithMessage("Customer ID is required.")
      .MustAsync(CustomerExists)
        .WithMessage("Customer not found.");
  }

  private async Task<bool> CustomerExists(int CustomerId, CancellationToken cancellationToken)
  {
    return await _unitOfWork.Customers.AnyAsync(
      p => p.CustomerId == CustomerId,
      cancellationToken);
  }
}