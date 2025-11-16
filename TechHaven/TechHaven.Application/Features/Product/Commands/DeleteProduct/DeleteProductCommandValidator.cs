using FluentValidation;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
  private readonly IUnitOfWork _unitOfWork;

  public DeleteProductCommandValidator(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;

    RuleFor(x => x.ProductId)
      .GreaterThan(0).WithMessage("Product ID is required.")
      .MustAsync(ProductExists)
        .WithMessage("Product not found.")
      .MustAsync(ProductNotInOrders)
        .WithMessage("Cannot delete product that exists in orders. Consider marking it as draft instead.");
  }

  private async Task<bool> ProductExists(int productId, CancellationToken cancellationToken)
  {
    return await _unitOfWork.Products.AnyAsync(
      p => p.ProductId == productId,
      cancellationToken);
  }

  private async Task<bool> ProductNotInOrders(int productId, CancellationToken cancellationToken)
  {
    // Check if product exists in any order details
    var hasOrders = await _unitOfWork.Products.AnyAsync(
      p => p.ProductId == productId && p.OrderDetails != null && p.OrderDetails.Any(),
      cancellationToken);

    return !hasOrders;
  }
}