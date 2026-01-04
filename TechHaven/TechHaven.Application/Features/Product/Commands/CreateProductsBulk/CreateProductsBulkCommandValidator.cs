// TechHaven.Application/Features/Product/Commands/CreateProductsBulk/CreateProductsBulkCommandValidator.cs
using FluentValidation;

namespace TechHaven.Application.Features.Product.Commands.CreateProductsBulk;

public class CreateProductsBulkCommandValidator : AbstractValidator<CreateProductsBulkCommand>
{
  public CreateProductsBulkCommandValidator()
  {
    RuleFor(x => x.Products)
      .NotEmpty().WithMessage("Products list cannot be empty")
      .Must(products => products != null && products.Count > 0)
      .WithMessage("At least one product is required");

    RuleFor(x => x.Products.Count)
      .LessThanOrEqualTo(1000)
      .WithMessage("Maximum 1000 products can be imported at once");
  }
}