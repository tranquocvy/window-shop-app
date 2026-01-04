namespace TechHaven.Shared.DTOs.Products;

public class ProductBulkCreateResponseDto
{
  /// <summary>
  /// Total number of products in the request
  /// </summary>
  public int TotalProducts { get; set; }

  /// <summary>
  /// Number of products successfully created
  /// </summary>
  public int SuccessCount { get; set; }

  /// <summary>
  /// Number of products failed to create
  /// </summary>
  public int FailedCount { get; set; }

  /// <summary>
  /// Number of products skipped (duplicates)
  /// </summary>
  public int SkippedCount { get; set; }

  /// <summary>
  /// Details of created products
  /// </summary>
  public List<ProductDto> CreatedProducts { get; set; } = new();

  /// <summary>
  /// Validation errors for failed products
  /// </summary>
  public List<ProductImportError> Errors { get; set; } = new();
}

/// <summary>
/// Error details for a failed product import
/// </summary>
public class ProductImportError
{
  /// <summary>
  /// Row index in the original Excel file (1-based)
  /// </summary>
  public int RowIndex { get; set; }

  /// <summary>
  /// Product name (for reference)
  /// </summary>
  public string ProductName { get; set; } = string.Empty;

  /// <summary>
  /// Error messages
  /// </summary>
  public List<string> ErrorMessages { get; set; } = new();
}