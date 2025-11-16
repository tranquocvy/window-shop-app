using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Interfaces;
using AutoMapper;

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
      request.SearchTerm,
      request.IsDraft,
      request.PageNumber,
      request.PageSize,
      request.SortBy,
      request.SortDescending,
      cancellationToken
    );

    var productsDto = _mapper.Map<IReadOnlyList<ProductDto>>(products);

    return new PagingResponse<ProductDto>
    {
      Items = productsDto,
      PageNumber = request.PageNumber,
      PageSize = request.PageSize,
      TotalCount = totalCount
    };
  }
}