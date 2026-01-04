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
        DateTime? toDateEndOfDay = null;

        if (criteria.ToDate.HasValue)
        {
            // Lấy phần ngày (00:00) -> Cộng 1 ngày -> Trừ 1 Tick -> 23:59:59.9999
            var calculatedDate = criteria.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            toDateEndOfDay = DateTime.SpecifyKind(calculatedDate, DateTimeKind.Utc); // ép kiểu về Utc
        }
        return x =>
            (string.IsNullOrEmpty(criteria.CustomerKeyword) ||
             (x.Customer != null && x.Customer.CustomerName.Contains(criteria.CustomerKeyword))) &&

            (!criteria.CustomerId.HasValue || x.CustomerId == criteria.CustomerId) &&

            (!criteria.Status.HasValue || x.Status == criteria.Status) &&

            (!criteria.FromDate.HasValue || x.OrderDate >= criteria.FromDate) &&
            (!toDateEndOfDay.HasValue || x.OrderDate <= toDateEndOfDay);
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
    AddInclude(o => o.OrderDetails!);//load OrderDetails để tính tổng số lượng

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