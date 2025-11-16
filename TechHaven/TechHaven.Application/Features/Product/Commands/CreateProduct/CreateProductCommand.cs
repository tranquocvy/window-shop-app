using TechHaven.Application.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Application.Features.Product.Commands.CreateProduct;

public record CreateProductCommand() : ICommand<ProductDto>
{
  public string ProductName { get; init; } = string.Empty;
  public string BrandName { get; init; } = string.Empty;
  public string? Color { get; init; }
  public int? StorageCapacity { get; init; }
  public string? Processor { get; init; }
  public decimal? ScreenSize { get; init; }
  public int? BatteryCapacity { get; init; }
  public string? ImageUrl { get; init; }
  public string? ImageGalleryJson { get; init; }
  public decimal CostPrice { get; init; }
  public decimal SellPrice { get; init; }
  public int StockQuantity { get; init; }
  public string? Description { get; init; }
  public bool IsDraft { get; init; } = false;
}