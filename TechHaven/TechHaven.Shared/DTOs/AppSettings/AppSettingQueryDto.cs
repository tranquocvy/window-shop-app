using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.AppSettings;

public class AppSettingQueryDto : PagingRequest
{
    public string? Key { get; set; }

    public SettingType? ValueType { get; set; }

    public string? Category { get; set; }

    public bool? IsSystem { get; set; }

    public DateRangeFilter? UpdatedAt { get; set; }

    public SortingOption? Sorting { get; set; }
}