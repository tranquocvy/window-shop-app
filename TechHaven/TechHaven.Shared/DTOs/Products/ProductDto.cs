namespace TechHaven.Shared.DTOs.Products;

public class ProductDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string BrandName { get; set; } = string.Empty;

    public string? Color { get; set; }
    
    public int? StorageCapacity { get; set; }

    public string? Processor { get; set; }

    public decimal? ScreenSize { get; set; }

    public int? BatteryCapacity { get; set; }
    
    public string? ImageUrl { get; set; }

    public string? ImageGalleryJson { get; set; }

    public decimal SellPrice { get; set; }

    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public bool IsDraft { get; set; }
}