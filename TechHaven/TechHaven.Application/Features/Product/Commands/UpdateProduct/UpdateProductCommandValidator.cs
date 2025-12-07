using FluentValidation;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Domain.Interfaces;

/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
  public UpdateProductCommandValidator()
  {
    RuleFor(x => x.ProductId)
      .GreaterThan(0).WithMessage("Product ID is required.");

    RuleFor(x => x.ProductName)
      .NotEmpty().WithMessage("Product name is required.")
      .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

    RuleFor(x => x.SellPrice)
      .GreaterThanOrEqualTo(0).WithMessage("Sell price cannot be negative.");

    RuleFor(x => x.StockQuantity)
      .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

    RuleFor(x => x.Color)
      .MaximumLength(50).WithMessage("Color cannot exceed 50 characters.")
      .When(x => !string.IsNullOrWhiteSpace(x.Color));

    RuleFor(x => x.Processor)
      .MaximumLength(100).WithMessage("Processor cannot exceed 100 characters.")
      .When(x => !string.IsNullOrWhiteSpace(x.Processor));

    RuleFor(x => x.ScreenSize)
      .GreaterThan(0).WithMessage("Screen size must be greater than 0.")
      .When(x => x.ScreenSize.HasValue);

    RuleFor(x => x.StorageCapacity)
      .GreaterThan(0).WithMessage("Storage capacity must be greater than 0.")
      .When(x => x.StorageCapacity.HasValue);

    RuleFor(x => x.BatteryCapacity)
      .GreaterThan(0).WithMessage("Battery capacity must be greater than 0.")
      .When(x => x.BatteryCapacity.HasValue);

    RuleFor(x => x.BrandName)
      .NotEmpty().WithMessage("Brand name is required.")
      .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.");
  }
}