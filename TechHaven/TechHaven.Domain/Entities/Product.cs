namespace TechHaven.Domain.Entities;

public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string? Description { get; set; }
    public bool IsDraft { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Category? Category { get; set; }
}