using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Products;

public class ProductQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public string? CategoryName { get; set; }

    public bool? IsDraft { get; set; }

    public SortingOption? Sorting { get; set; }
}


