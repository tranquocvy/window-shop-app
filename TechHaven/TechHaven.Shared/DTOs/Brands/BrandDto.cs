namespace TechHaven.Shared.DTOs.Brands;

/// <summary>
/// DTO representing a product brand
/// Extracted from Product.BrandName field
/// </summary>
public class BrandDto
{
  /// <summary>
  /// Brand name (unique)
  /// </summary>
  public string BrandName { get; set; } = string.Empty;

  /// <summary>
  /// Number of products under this brand
  /// </summary>
  public int ProductCount { get; set; }

  /// <summary>
  /// Lowest price among products of this brand
  /// </summary>
  public decimal? MinPrice { get; set; }

  /// <summary>
  /// Highest price among products of this brand
  /// </summary>
  public decimal? MaxPrice { get; set; }

  /// <summary>
  /// Total stock quantity for all products of this brand
  /// </summary>
  public int TotalStock { get; set; }
}