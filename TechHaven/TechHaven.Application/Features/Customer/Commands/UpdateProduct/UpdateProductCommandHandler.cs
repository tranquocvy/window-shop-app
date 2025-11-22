using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using AutoMapper;
using TechHaven.Domain.Interfaces;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Application.Features.Customer.Commands.UpdateCustomer;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, CustomerDto>
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

  public async Task<CustomerDto> Handle(
    UpdateCustomerCommand request,
    CancellationToken cancellationToken)
  {
    // Get existing Customer
    var customer = await _unitOfWork.Customers.GetByIdAsync(
      request.CustomerId,
      cancellationToken);

    if (customer == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
    }

    // Update properties
    customer.CustomerName = request.CustomerName;
    customer.PhoneNumber = request.PhoneNumber;
    customer.Email = request.Email;
    customer.Address = request.Address;
    customer.Type = (TechHaven.Domain.Enums.CustomerType)request.Type;
    customer.TotalPurchased = request.TotalPurchased;
    customer.Note = request.Note;

    await _unitOfWork.Customers.UpdateAsync(customer);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<CustomerDto>(customer);
  }
}