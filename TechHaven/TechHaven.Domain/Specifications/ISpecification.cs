using System.Linq.Expressions;

namespace TechHaven.Domain.Specifications;

public interface ISpecification<T>
{
  // Criteria expression để filter
  Expression<Func<T, bool>>? Criteria { get; }

  // Danh sách includes cho eager loading
  List<Expression<Func<T, object>>> Includes { get; }

  // OrderBy expression
  Expression<Func<T, object>>? OrderBy { get; }
  Expression<Func<T, object>>? OrderByDescending { get; }

  // Paging
  int Take { get; }
  int Skip { get; }
  bool IsPagingEnabled { get; }
}