

using FluentValidation;
using TechHaven.Application.Features.Product.Commands.UpdateProduct;
using TechHaven.Domain.Interfaces;


/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
  private readonly IUnitOfWork _unitOfWork;

  public UpdateProductCommandValidator(IUnitOfWork unitOfWork)
  {
    _unitOfWork = unitOfWork;

    RuleFor(x => x.productDto.ProductId)
      .GreaterThan(0).WithMessage("Product ID is required.")
      .MustAsync(ProductExists)
          .WithMessage("Product not found.");

    RuleFor(x => x.productDto.ProductName)
      .NotEmpty().WithMessage("Product name is required.")
      .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.")
          .WithMessage("A product with this name already exists in the selected category.");

    RuleFor(x => x.productDto.SellPrice)
    .GreaterThanOrEqualTo(0).WithMessage("Sell price cannot be negative.")
    .WithMessage("Sell price must be greater than or equal to cost price.");

    RuleFor(x => x.productDto.StockQuantity)
    .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

    RuleFor(x => x.productDto.Color)
      .MaximumLength(50).WithMessage("Color cannot exceed 50 characters.")
      .When(x => !string.IsNullOrWhiteSpace(x.productDto.Color));

    RuleFor(x => x.productDto.Processor)
      .MaximumLength(100).WithMessage("Processor cannot exceed 100 characters.")
      .When(x => !string.IsNullOrWhiteSpace(x.productDto.Processor));

    RuleFor(x => x.productDto.ScreenSize)
      .GreaterThan(0).WithMessage("Screen size must be greater than 0.")
      .When(x => x.productDto.ScreenSize.HasValue);

    RuleFor(x => x.productDto.StorageCapacity)
      .GreaterThan(0).WithMessage("Storage capacity must be greater than 0.")
      .When(x => x.productDto.StorageCapacity.HasValue);

    RuleFor(x => x.productDto.BatteryCapacity)
      .GreaterThan(0).WithMessage("Battery capacity must be greater than 0.")
      .When(x => x.productDto.BatteryCapacity.HasValue);

    RuleFor(x => x.productDto.BrandName)
      .NotEmpty().WithMessage("Brand name is required.")
      .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.");
  }

  private async Task<bool> ProductExists(int productId, CancellationToken cancellationToken)
  {
    return await _unitOfWork.Products.AnyAsync(
        p => p.ProductId == productId,
        cancellationToken);
  }
}