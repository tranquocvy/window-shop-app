using TechHaven.Shared.DTOs.Products;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Application.Features.Product.Queries.GetProducts;

public record GetProductsQuery(ProductSearchCriteria criteria) : IQuery<PagingResponse<ProductDto>>;