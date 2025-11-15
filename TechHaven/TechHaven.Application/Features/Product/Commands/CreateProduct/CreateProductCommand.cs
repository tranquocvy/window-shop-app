using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Product.Commands.CreateProduct;

public record CreateProductCommand(ProductCreateUpdateDto productDto) : ICommand<ProductDto>;