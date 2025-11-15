using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Product.Commands.UpdateProduct;

public record UpdateProductCommand(ProductDto productDto) : ICommand<ProductDto>;