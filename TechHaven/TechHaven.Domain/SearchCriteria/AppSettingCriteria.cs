namespace TechHaven.Domain.SearchCriteria;

public class AppSettingSearchCriteria
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Tìm kiếm theo Key hoặc Category
    public string? SearchTerm { get; set; }

    // Filter theo UserId (để lấy setting của user này hoặc system)
    public int? UserId { get; set; }
}