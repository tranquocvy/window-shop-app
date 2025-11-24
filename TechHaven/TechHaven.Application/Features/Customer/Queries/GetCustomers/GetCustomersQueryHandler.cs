using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Domain.Interfaces;
using AutoMapper;

namespace TechHaven.Application.Features.Customer.Queries.GetCustomers;

public class GetCustomersQueryHandler : IQueryHandler<GetCustomersQuery, PagingResponse<CustomerDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<PagingResponse<CustomerDto>> Handle(
    GetCustomersQuery request,
    CancellationToken cancellationToken)
  {
    var (customers, totalCount) = await _unitOfWork.Customers.SearchWithPaginationAsync(
      request.criteria,
      cancellationToken
    );

    var CustomersDto = _mapper.Map<IReadOnlyList<CustomerDto>>(customers);

    return new PagingResponse<CustomerDto>
    {
      Items = CustomersDto,
      PageNumber = request.criteria.PageNumber,
      PageSize = request.criteria.PageSize,
      TotalCount = totalCount
    };
  }
}