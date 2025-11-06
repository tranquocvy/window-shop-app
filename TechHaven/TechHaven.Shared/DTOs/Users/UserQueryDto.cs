using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Users;

public class UserQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }

    public SortingOption? Sorting { get; set; }
}


