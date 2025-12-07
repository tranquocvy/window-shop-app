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
    // Xử lý Time Component cho ToDate
    // Nếu có ToDate, ta dịch nó về giây cuối cùng của ngày đó (hoặc so sánh nhỏ hơn ngày hôm sau)
    DateTime? toDateEndOfDay = criteria.ToDate.HasValue
        ? criteria.ToDate.Value.Date.AddDays(1).AddTicks(-1) // 23:59:59.9999
        : null;

    // FromDate thường không cần sửa nếu mặc định là 00:00:00

    return x =>
        // Truy cập vào Navigation Property: x.Customer.CustomerName
        (string.IsNullOrEmpty(criteria.CustomerKeyword) ||
         (x.Customer != null && x.Customer.CustomerName.Contains(criteria.CustomerKeyword))) &&

        (!criteria.CustomerId.HasValue || x.CustomerId == criteria.CustomerId) &&

        (!criteria.Status.HasValue || x.Status == criteria.Status) &&

        (!criteria.FromDate.HasValue || x.OrderDate >= criteria.FromDate) &&
        (!criteria.ToDate.HasValue || x.OrderDate <= criteria.ToDate);
    // &&(!criteria.MinTotalAmount.HasValue || x.TotalAmount >= criteria.MinTotalAmount) 
    // &&(!criteria.MaxTotalAmount.HasValue || x.TotalAmount <= criteria.MaxTotalAmount);
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
    AddInclude(o => o.Customer!);
    AddInclude(o => o.User!);

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
      "customer" => (Expression<Func<Order, object>>)(x => x.Customer != null ? x.Customer.CustomerName : string.Empty), // Sort theo tên khách, Default: Sort theo OrderId cũng sẽ đi vào sorting này
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
    AddInclude(o => o.Customer!);
    AddInclude(o => o.User!);
    AddInclude(o => o.OrderDetails!);
    AddInclude(o => o.Payments!);
  }
}

public class OrdersByProductAndDateRangeSpecification : BaseSpecification<Order>
{
  public OrdersByProductAndDateRangeSpecification(
    int productId,
    DateTime startDate,
    DateTime endDate
  ) : base(BuildCriteria(productId, startDate, endDate))
  {
    AddInclude(o => o.OrderDetails!);
  }

  private static Expression<Func<Order, bool>>? BuildCriteria(int productId, DateTime startDate, DateTime endDate)
  {
    return o =>
      o.OrderDetails != null &&
      o.OrderDetails.Any(od => od.ProductId == productId) &&
      o.OrderDate >= startDate &&
      o.OrderDate <= endDate &&
      o.Status == OrderStatus.Completed;
  }
}