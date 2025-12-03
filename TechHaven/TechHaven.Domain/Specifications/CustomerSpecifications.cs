using System.Linq.Expressions;
using TechHaven.Domain.Entities;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Specifications;

public class CustomerSearchSpecification : BaseSpecification<Customer>
{
  public CustomerSearchSpecification(CustomerSearchCriteria criteria)
    : base(BuildCriteria(criteria.SearchTerm))
  {
    ApplySorting(criteria.SortBy, criteria.SortDescending);
    ApplyPaging((criteria.PageNumber - 1) * criteria.PageSize, criteria.PageSize);
  }

  public CustomerSearchSpecification(string? searchTerm)
    : base(BuildCriteria(searchTerm))
  {
    // Không làm gì cả, chỉ giữ Criteria từ base
  }

  private static Expression<Func<Customer, bool>>? BuildCriteria(string? searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
      return null;

    return c =>
      c.CustomerName.ToLower().Contains(searchTerm.ToLower()) ||
      c.PhoneNumber.Contains(searchTerm) ||
      (c.Email != null && c.Email.Contains(searchTerm)) ||
      (c.Address != null && c.Address.ToLower().Contains(searchTerm.ToLower()));
  }

  private void ApplySorting(string? sortBy, bool sortDescending)
  {
    // var sorting = sortBy?.ToLower() switch
    // {
    //   "name" => (Expression<Func<Customer, object>>)(c => c.CustomerName),
    //   "totalpurchased" => (Expression<Func<Customer, object>>)(c => c.TotalPurchased),
    //   ""
    //   _ => (Expression<Func<Customer, object>>)(c => c.CustomerName)
    // };

    // if (descending)
    //   ApplyOrderByDescending(sorting);
    // else
    //   ApplyOrderBy(sorting);
    if (string.IsNullOrWhiteSpace(sortBy))
    {
      if (sortDescending)
        ApplyOrderByDescending(p => p.CustomerName);
      else
        ApplyOrderBy(p => p.CustomerName);
      return;
    }

    switch (sortBy.ToLower())
    {
      case "name":
      case "customername":
        if (sortDescending)
          ApplyOrderByDescending(p => p.CustomerName);
        else
          ApplyOrderBy(p => p.CustomerName);
        break;

      case "id":
      case "customerid":
        if (sortDescending)
          ApplyOrderByDescending(p => p.CustomerId);
        else
          ApplyOrderBy(p => p.CustomerId);
        break;

      case "phone":
      case "phonenumber":
        if (sortDescending)
          ApplyOrderByDescending(p => p.PhoneNumber);
        else
          ApplyOrderBy(p => p.PhoneNumber);
        break;

      case "email":
        if (sortDescending)
          ApplyOrderByDescending(p => p.Email ?? string.Empty);
        else
          ApplyOrderBy(p => p.Email ?? string.Empty);
        break;  

      case "address":
        if (sortDescending)
          ApplyOrderByDescending(p => p.Address ?? string.Empty);
        else
          ApplyOrderBy(p => p.Address ?? string.Empty);
        break;

      case "type":
        if (sortDescending)
          ApplyOrderByDescending(p => p.Type);
        else
          ApplyOrderBy(p => p.Type);
        break;

      case "totalpurchased":
        if (sortDescending)
          ApplyOrderByDescending(p => p.TotalPurchased);
        else
          ApplyOrderBy(p => p.TotalPurchased);
        break;

      default:
        // Default to ProductName if invalid sortBy
        if (sortDescending)
          ApplyOrderByDescending(p => p.CustomerName);
        else
          ApplyOrderBy(p => p.CustomerName);
        break;
    }
  }
}