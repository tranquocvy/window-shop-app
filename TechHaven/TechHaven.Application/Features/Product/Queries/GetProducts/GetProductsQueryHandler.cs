using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Interfaces;
using AutoMapper;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Application.Features.Product.Queries.GetProducts;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagingResponse<ProductDto>>
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
  {
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<PagingResponse<ProductDto>> Handle(
    GetProductsQuery request,
    CancellationToken cancellationToken)
  {
    var (products, totalCount) = await _unitOfWork.Products.SearchWithPaginationAsync(
      request.criteria,
      cancellationToken
    );

    var productsDto = _mapper.Map<IReadOnlyList<ProductDto>>(products);

    return new PagingResponse<ProductDto>
    {
      Items = productsDto,
      PageNumber = request.criteria.PageNumber,
      PageSize = request.criteria.PageSize,
      TotalCount = totalCount
    };
  }
}