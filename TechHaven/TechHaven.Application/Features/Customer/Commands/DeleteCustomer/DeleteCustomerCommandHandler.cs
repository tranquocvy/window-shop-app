using TechHaven.Application.Interfaces;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;
using MediatR;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand, Result>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteCustomerCommandHandler(
    IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;
  }

  public async Task<Result> Handle(
    DeleteCustomerCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);

      if (customer == null)
      {
        return Result.Failure(
          $"Customer {request.CustomerId} not found.",
          ErrorType.NotFound
        );
      }

      await _unitOfWork.Customers.DeleteAsync(customer, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      return Result.Success();
    }
    catch (Exception ex)
    {
      return Result.Failure(
        $"Failed to delete customer: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}