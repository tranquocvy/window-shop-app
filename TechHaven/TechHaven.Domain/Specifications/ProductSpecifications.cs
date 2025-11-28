using System.Linq.Expressions;
using TechHaven.Domain.Entities;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Specifications;

/// <summary>
/// Specification for getting products with basic filtering and sorting
/// </summary>
public class ProductSearchSpecification : BaseSpecification<Product>
{
  public ProductSearchSpecification(
    ProductSearchCriteria criteria, bool hasPaging)
    : base(BuildCriteria(criteria))
  {
    // Apply sorting
    ApplySortingLogic(criteria.SortBy, criteria.SortDescending);

    // Apply paging
    // Nếu ko có tham số hasPaging -> tự động paging
    if (hasPaging)
    {
      ApplyPaging((criteria.PageNumber - 1) * criteria.PageSize, criteria.PageSize);
    }
  }

  // // Todo: Check lại logic cái này: Hiện chưa filter nếu muốn lấy all
  // public ProductSearchSpecification(string? searchTerm, bool? isDraft)
  //   : base(BuildCriteriaSimple(searchTerm, isDraft))
  // {
  //   // Không làm gì cả, chỉ giữ Criteria từ base
  // }

  private static Expression<Func<Product, bool>>? BuildCriteria(ProductSearchCriteria criteria)
  {
    bool hasValidStatus = criteria.Status.HasValue && (int)criteria.Status.Value > 0;

    // Nếu không có filter nào => trả về null (lấy tất cả)
    if (string.IsNullOrWhiteSpace(criteria.SearchTerm)
        && !criteria.IsDraft.HasValue
        && !criteria.FromPrice.HasValue
        && !criteria.ToPrice.HasValue
        && string.IsNullOrWhiteSpace(criteria.Brand)
        && !hasValidStatus)
      return null;

    return p =>
      // Filter: IsDraft
      (!criteria.IsDraft.HasValue || p.IsDraft == criteria.IsDraft.Value) &&

      // Filter: SearchTerm (tên, hãng, mô tả)
      (string.IsNullOrWhiteSpace(criteria.SearchTerm) ||
       p.ProductName.ToLower().Contains(criteria.SearchTerm.ToLower()) ||
       p.BrandName.Contains(criteria.SearchTerm.ToLower())) &&

      // Filter: Khoảng giá (FromPrice -> ToPrice)
      (!criteria.FromPrice.HasValue || p.SellPrice >= criteria.FromPrice.Value) &&
      (!criteria.ToPrice.HasValue || p.SellPrice <= criteria.ToPrice.Value) &&

      // Filter: Hãng
      (string.IsNullOrWhiteSpace(criteria.Brand) || p.BrandName.ToLower() == criteria.Brand.ToLower()) &&

      // Filter: Trạng thái (còn hàng / hết hàng)
      (!hasValidStatus ||
       (criteria.Status == ProductStatus.InStock && p.StockQuantity > 0) ||
       (criteria.Status == ProductStatus.OutOfStock && p.StockQuantity == 0));
  }

  private void ApplySortingLogic(string? sortBy, bool sortDescending)
  {
    if (string.IsNullOrWhiteSpace(sortBy))
    {
      // Default sorting by ProductName
      if (sortDescending)
        ApplyOrderByDescending(p => p.ProductName);
      else
        ApplyOrderBy(p => p.ProductName);
      return;
    }

    switch (sortBy.ToLower())
    {
      case "name":
      case "productname":
        if (sortDescending)
          ApplyOrderByDescending(p => p.ProductName);
        else
          ApplyOrderBy(p => p.ProductName);
        break;

      case "price":
      case "sellprice":
        if (sortDescending)
          ApplyOrderByDescending(p => p.SellPrice);
        else
          ApplyOrderBy(p => p.SellPrice);
        break;

      case "stock":
      case "stockquantity":
        if (sortDescending)
          ApplyOrderByDescending(p => p.StockQuantity);
        else
          ApplyOrderBy(p => p.StockQuantity);
        break;

      case "brand":
      case "brandname":
        if (sortDescending)
          ApplyOrderByDescending(p => p.BrandName);
        else
          ApplyOrderBy(p => p.BrandName);
        break;

      case "createdat":
        if (sortDescending)
          ApplyOrderByDescending(p => p.CreatedAt);
        else
          ApplyOrderBy(p => p.CreatedAt);
        break;

      default:
        // Default to ProductName if invalid sortBy
        if (sortDescending)
          ApplyOrderByDescending(p => p.ProductName);
        else
          ApplyOrderBy(p => p.ProductName);
        break;
    }
  }
}

// /// <summary>
// /// Specification for getting a single product by ID
// /// </summary>
// public class ProductByIdSpecification : BaseSpecification<Product>
// {
//   public ProductByIdSpecification(int productId)
//       : base(p => p.ProductId == productId)
//   {
//   }
// }

/// <summary>
/// Specification for getting products with low stock
/// </summary>
public class LowStockProductsSpecification : BaseSpecification<Product>
{
  public LowStockProductsSpecification(int threshold = 10)
      : base(p => p.StockQuantity <= threshold && !p.IsDraft)
  {
    ApplyOrderBy(p => p.StockQuantity);
  }
}

// /// <summary>
// /// Specification for getting products by brand
// /// </summary>
// public class ProductsByBrandSpecification : BaseSpecification<Product>
// {
//   public ProductsByBrandSpecification(string brandName)
//       : base(p => p.BrandName == brandName && !p.IsDraft)
//   {
//     ApplyOrderBy(p => p.ProductName);
//   }
// }

// /// <summary>
// /// Specification for getting draft products
// /// </summary>
// public class DraftProductsSpecification : BaseSpecification<Product>
// {
//   public DraftProductsSpecification()
//       : base(p => p.IsDraft)
//   {
//     ApplyOrderByDescending(p => p.UpdatedAt ?? DateTime.MinValue);
//   }
// }

// /// <summary>
// /// Specification for getting products with order details (eager loading)
// /// </summary>
// public class ProductsWithOrderDetailsSpecification : BaseSpecification<Product>
// {
//   public ProductsWithOrderDetailsSpecification()
//       : base(null)
//   {
//     AddInclude(p => p.OrderDetails != null ? p.OrderDetails : Enumerable.Empty<OrderDetail>());
//   }
// }