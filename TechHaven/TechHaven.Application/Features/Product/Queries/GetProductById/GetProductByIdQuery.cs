using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IQuery<Result<ProductDto>>;