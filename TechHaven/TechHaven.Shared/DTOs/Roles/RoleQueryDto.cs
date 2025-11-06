using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Roles;

public class RoleQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public SortingOption? Sorting { get; set; }
}


