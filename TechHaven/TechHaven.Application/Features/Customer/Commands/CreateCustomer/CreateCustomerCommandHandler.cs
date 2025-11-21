using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using AutoMapper;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Customer.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public CreateCustomerCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<CustomerDto> Handle(
    CreateCustomerCommand request,
    CancellationToken cancellationToken)
  {
    // map command to entity
    var customer = _mapper.Map<Domain.Entities.Customer>(request);

    // set timestamps
    customer.CreatedAt = DateTime.Now;
    customer.UpdatedAt = null;

    // add to repository
    await _unitOfWork.Customers.AddAsync(customer, cancellationToken);

    // save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return _mapper.Map<CustomerDto>(customer);
  }
}