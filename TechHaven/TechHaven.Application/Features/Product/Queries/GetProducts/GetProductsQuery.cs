using TechHaven.Shared.DTOs.Products;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Application.Interfaces;

namespace TechHaven.Application.Features.Product.Queries.GetProducts;

public record GetProductsQuery : IQuery<PagingResponse<ProductDto>>
{
  public string? SearchTerm { get; init; }
  public bool? IsDraft { get; init; }
  public int PageNumber { get; init; } = 1;
  public int PageSize { get; init; } = 20;
  public string SortBy { get; init; } = nameof(ProductDto.ProductName);
  public bool SortDescending { get; init; } = false;
}