using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Customers;

public class CustomerQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public CustomerType? Type { get; set; }

    public DateRangeFilter? CreatedAt { get; set; }

    public SortingOption? Sorting { get; set; }
}