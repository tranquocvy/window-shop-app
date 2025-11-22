using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;
using MediatR;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteCustomerCommandHandler(
    IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Unit> Handle(
    DeleteCustomerCommand request,
    CancellationToken cancellationToken)
  {
    var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);

    if (customer == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
    }

    await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Unit.Value;
  }
}