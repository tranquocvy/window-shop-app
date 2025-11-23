using TechHaven.Shared.DTOs.Products;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Application.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Queries.GetProducts;

public record GetProductsQuery(ProductSearchCriteria criteria) : IQuery<Result<PagingResponse<ProductDto>>>;