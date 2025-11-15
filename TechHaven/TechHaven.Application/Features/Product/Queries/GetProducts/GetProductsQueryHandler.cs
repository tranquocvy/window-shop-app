using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Interfaces;
using AutoMapper;
using TechHaven.Application.Common.Helper;
using Microsoft.EntityFrameworkCore;

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
    var productsQuery = _unitOfWork.Products.Search(
      searchTerm: request.SearchTerm,
      isDraft: request.IsDraft
    );

    productsQuery = SortingHelper.ApplySorting(
      productsQuery,
      request.SortBy,
      request.SortDescending
    );

    int totalCount = productsQuery.Count();

    var items = await productsQuery
      .Skip((request.PageNumber - 1) * request.PageSize)
      .Take(request.PageSize)
      .ToListAsync(cancellationToken);

    var productsDto = _mapper.Map<List<ProductDto>>(items);

    return new PagingResponse<ProductDto>
    {
      Items = productsDto,
      PageNumber = request.PageNumber,
      PageSize = request.PageSize,
      TotalCount = totalCount
    };
  }
}