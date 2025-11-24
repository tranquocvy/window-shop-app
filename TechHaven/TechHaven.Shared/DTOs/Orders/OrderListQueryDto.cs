using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Orders;

public class OrderListQueryDto : PagingRequest
{

    public OrderStatus? Status { get; set; }

    public DateRangeFilter? OrderDate { get; set; }

    public string? CustomerKeyword { get; set; }

    public SortingOption? Sorting { get; set; }
}