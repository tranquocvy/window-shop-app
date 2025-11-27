using System.Linq.Expressions;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Specifications;


//Helper counting -> DRY principle
public static class OrderFilterBuilder
{
    /// <summary>
    /// static function return the common Expression for Entity Order
    /// </summary>
    public static Expression<Func<Order, bool>> Build(OrderSearchCriteria criteria)
    {
        return x =>
            (string.IsNullOrEmpty(criteria.SearchTerm) ||
                x.Notes.Contains(criteria.SearchTerm) ||
                x.OrderId.ToString().Contains(criteria.SearchTerm)) &&

            (!criteria.CustomerId.HasValue || x.CustomerId == criteria.CustomerId) &&

            (!criteria.Status.HasValue || x.Status == criteria.Status) &&

            (!criteria.FromDate.HasValue || x.OrderDate >= criteria.FromDate) &&

            (!criteria.ToDate.HasValue || x.OrderDate <= criteria.ToDate) &&

            (!criteria.MinTotalAmount.HasValue || x.TotalAmount >= criteria.MinTotalAmount) &&
            (!criteria.MaxTotalAmount.HasValue || x.TotalAmount <= criteria.MaxTotalAmount);
    }
}


//Feature 1
public class OrderSearchSpecification : BaseSpecification<Order>
{
    // Constructor dùng cho việc lấy dữ liệu (có Paging, Sorting, Include)
    public OrderSearchSpecification(OrderSearchCriteria criteria)
        : base(OrderFilterBuilder.Build(criteria))
    {
        // Chỉ Include cấp 1 những bảng cần hiện lên Grid
        AddInclude(o => o.Customer);
        AddInclude(o => o.User);

        ApplySorting(criteria.SortBy, criteria.SortDescending);
        ApplyPaging((criteria.PageNumber - 1) * criteria.PageSize, criteria.PageSize);
    }

    // Constructor dùng cho việc đếm tổng số bản ghi (Count) - Không cần Include hay Sorting
    public OrderSearchSpecification(OrderSearchCriteria criteria, bool forCounting)
        : base(OrderFilterBuilder.Build(criteria)) //đếm
    {
        // Constructor này chỉ để tái sử dụng Criteria cho hàm CountAsync
    }

    private void ApplySorting(string? sortBy, bool descending)
    {
        var sorting = sortBy?.ToLower() switch
        {
            "totalamount" => (Expression<Func<Order, object>>)(x => x.TotalAmount),
            "date" or "orderdate" => (Expression<Func<Order, object>>)(x => x.OrderDate),
            "status" => (Expression<Func<Order, object>>)(x => x.Status),
            "customer" => (Expression<Func<Order, object>>)(x => x.Customer.CustomerName), // Sort theo tên khách
            _ => (Expression<Func<Order, object>>)(x => x.OrderDate) // Mặc định sort theo ngày
        };

        if (descending)
            ApplyOrderByDescending(sorting);
        else
            ApplyOrderBy(sorting);
    }
}

//Feature 2
public class OrderDetailSpecification : BaseSpecification<Order>
{
    public OrderDetailSpecification(int orderId)
        : base(o => o.OrderId == orderId)
    {
        // Include cấp 1, warning có thể bỏ qua
        AddInclude(o => o.Customer);
        AddInclude(o => o.User);
        AddInclude(o => o.OrderDetails);
        AddInclude(o => o.Payments);

        // VẤN ĐỀ: BaseSpecification KHÔNG hỗ trợ ThenInclude (VD: OrderDetails.Product).
        // -> Xử lý vấn đề này ở tầng Repository 
    }
}
