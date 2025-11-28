using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Products;

public class ProductListQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public bool? IsDraft { get; set; }

    // Lọc theo khoảng giá
    public decimal? FromPrice { get; set; }
    public decimal? ToPrice { get; set; }

    // Lọc theo hãng
    public string? Brand { get; set; }

    // Lọc theo trạng thái
    public ProductStatus? Status { get; set; }

    public SortingOption? Sorting { get; set; }
}

/// <summary>
/// Defines the types of customers in the system.
/// </summary>
public enum ProductStatus
{
    InStock = 1, // Còn hàng
    OutOfStock = 2, // Hết hàng
}