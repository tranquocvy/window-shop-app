using TechHaven.Application.Interfaces;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : ICommand;