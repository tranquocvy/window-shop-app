using AutoMapper;

using TechHaven.Domain.Interfaces;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Application.Common.Exceptions;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
  {
    // Try to find Customer
    var customer = await _unitOfWork.Customers.GetByIdAsync(
      request.CustomerId,
      cancellationToken
    );

    if (customer == null)
    {
      throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId);
    }

    // Map to DTO
    return _mapper.Map<CustomerDto>(customer);
  }
}