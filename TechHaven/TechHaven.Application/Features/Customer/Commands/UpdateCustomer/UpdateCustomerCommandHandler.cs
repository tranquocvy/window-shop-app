using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using AutoMapper;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public UpdateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<CustomerDto>> Handle(
    UpdateCustomerCommand request,
    CancellationToken cancellationToken)
  {
    try
    {
      // Get existing Customer
      var customer = await _unitOfWork.Customers.GetByIdAsync(
        request.CustomerId,
        cancellationToken);

      if (customer == null)
      {
        return Result<CustomerDto>.Failure(
          $"Product {request.CustomerName} not found.",
          ErrorType.NotFound
        );
      }

      // Update properties
      _mapper.Map(request, customer);
      customer.UpdatedAt = DateTime.Now;

      await _unitOfWork.Customers.UpdateAsync(customer);
      await _unitOfWork.SaveChangesAsync();

      var customerDto = _mapper.Map<CustomerDto>(customer);
      return Result<CustomerDto>.Success(customerDto);
    }
    catch (Exception ex)
    {
      return Result<CustomerDto>.Failure(
        $"Failed to update customer: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}