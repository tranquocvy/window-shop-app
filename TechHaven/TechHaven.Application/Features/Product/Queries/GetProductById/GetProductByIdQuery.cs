using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Product.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IQuery<ProductDto>;