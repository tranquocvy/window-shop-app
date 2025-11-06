using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Categories;

public class CategoryQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public SortingOption? Sorting { get; set; }
}