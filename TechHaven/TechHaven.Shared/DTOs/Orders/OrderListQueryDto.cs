using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Shared.DTOs.Orders;

public class OrderListQueryDto : PagingRequest
{
    //TODO: Xem xét lại việc kế thừa PagingRequest nếu không cần thiết vì nó chỉ dùng để truy vấn dữ liệu (GET) chứ không phải POST/PUT
    public PagingRequest pageRequest { get; set; } = new PagingRequest();

    public OrderStatus? Status { get; set; }

    public DateRangeFilter? OrderDate { get; set; }

    public string? CustomerKeyword { get; set; }

    public SortingOption? Sorting { get; set; }
}