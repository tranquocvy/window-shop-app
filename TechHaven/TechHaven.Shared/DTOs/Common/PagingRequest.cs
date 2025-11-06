namespace TechHaven.Shared.DTOs.Common;

public class PagingRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}