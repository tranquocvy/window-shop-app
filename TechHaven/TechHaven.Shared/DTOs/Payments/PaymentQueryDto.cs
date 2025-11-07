using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Payments;

public class PaymentQueryDto : PagingRequest
{
    public int? OrderId { get; set; }

    public DateRangeFilter? PaymentDate { get; set; }

    public SortingOption? Sorting { get; set; }
}