using TechHaven.Application.Interfaces;
using TechHaven.Domain.Common;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : ICommand<Result>;