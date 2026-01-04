using AutoMapper;

using TechHaven.Domain.Interfaces;
using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Application.Common.Exceptions;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetCustomerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
  {
    try
    {
      // Try to find Customer
      var customer = await _unitOfWork.Customers.GetByIdAsync(
        request.CustomerId,
        cancellationToken
      );

      if (customer == null)
      {
        return Result<CustomerDto>.Failure(
          $"Customer with ID {request.CustomerId} not found",
          ErrorType.NotFound);
      }

      // Map to DTO
      return _mapper.Map<CustomerDto>(customer);
    }
    catch (Exception ex)
    {
      return Result<CustomerDto>.Failure(
        $"Failed to get Customer with id {request.CustomerId}: {ex.Message}",
        ErrorType.InternalError);
    }
  }
}