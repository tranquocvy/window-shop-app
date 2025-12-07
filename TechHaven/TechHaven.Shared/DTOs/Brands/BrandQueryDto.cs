namespace TechHaven.Shared.DTOs.Brands;

/// <summary>
/// Request DTO for brand statistics query
/// </summary>
public class BrandQueryDto
{
  /// <summary>
  /// Search by brand name
  /// </summary>
  public string? SearchTerm { get; set; }

  /// <summary>
  /// Include only brands with products in stock
  /// </summary>
  public bool? InStockOnly { get; set; }

  /// <summary>
  /// Sort by: BrandName, ProductCount, MinPrice, MaxPrice
  /// </summary>
  public string? SortBy { get; set; }

  /// <summary>
  /// Sort descending
  /// </summary>
  public bool SortDescending { get; set; } = false;
}