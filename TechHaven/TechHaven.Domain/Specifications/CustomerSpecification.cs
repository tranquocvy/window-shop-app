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
      (c.Email != null && c.Email.ToLower().Contains(searchTerm.ToLower()));
  }

  private void ApplySorting(string? sortBy, bool descending)
  {
    var sorting = sortBy?.ToLower() switch
    {
      "name" => (Expression<Func<Customer, object>>)(c => c.CustomerName),
      "totalpurchased" => (Expression<Func<Customer, object>>)(c => c.TotalPurchased),
      _ => (Expression<Func<Customer, object>>)(c => c.CustomerName)
    };

    if (descending)
      ApplyOrderByDescending(sorting);
    else
      ApplyOrderBy(sorting);
  }
}