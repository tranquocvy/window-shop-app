using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Commissions;

public class CommissionQueryDto : PagingRequest
{
    public int? UserId { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public DateRangeFilter? CreatedAt { get; set; }

    public SortingOption? Sorting { get; set; }
}