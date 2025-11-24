using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Products;

public class ProductListQueryDto : PagingRequest
{
    public PagingRequest PagingRequest { get; set; } = new PagingRequest();
    public string? SearchTerm { get; set; }

    public bool? IsDraft { get; set; }

    public SortingOption? Sorting { get; set; }
}