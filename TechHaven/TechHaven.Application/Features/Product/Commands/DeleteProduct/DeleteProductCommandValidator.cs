using FluentValidation;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Application.Features.Product.Commands.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
  public DeleteProductCommandValidator()
  {
    RuleFor(x => x.ProductId)
      .GreaterThan(0).WithMessage("Product ID is required.");
  }
}