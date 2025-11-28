using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Products;

public class ProductListQueryDto : PagingRequest
{
    public string? SearchTerm { get; set; }

    public bool? IsDraft { get; set; }

    //Lọc theo khoảng giá
    public int? fromPrice { get; set; }
    public int? toPrice { get; set; }

    //Lọc theo hãng
    public string? Brand { get; set; }

    //Lọc theo trạng thái
    public string? Status { get; set; }

    public SortingOption? Sorting { get; set; }
}