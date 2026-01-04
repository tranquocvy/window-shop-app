namespace TechHaven.Shared.DTOs.Common;

public class PagingResponse<TItem>
{
    public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}