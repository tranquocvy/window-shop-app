namespace TechHaven.Shared.DTOs.Products;

public class ProductBulkCreateRequestDto
{
  /// <summary>
  /// List of products to create
  /// </summary>
  public List<ProductUpsertRequest> Products { get; set; } = new();

  /// <summary>
  /// Whether to skip products that already exist (based on ProductName)
  /// Default: true (skip duplicates)
  /// </summary>
  public bool SkipDuplicates { get; set; } = true;

  /// <summary>
  /// Whether to validate all products before inserting any
  /// Default: true (atomic operation)
  /// </summary>
  public bool ValidateBeforeInsert { get; set; } = true;
}