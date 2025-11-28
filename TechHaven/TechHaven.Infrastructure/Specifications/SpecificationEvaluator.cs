using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Specifications;

namespace TechHaven.Infrastructure.Specifications;

public static class SpecificationEvaluator<T> where T : class
{
  public static IQueryable<T> GetQuery(
      IQueryable<T> inputQuery,
      ISpecification<T> specification)
  {
    var query = inputQuery;

    // Apply criteria
    if (specification.Criteria != null)
    {
      query = query.Where(specification.Criteria);
    }

    // Apply includes (eager loading)
    query = specification.Includes
        .Aggregate(query, (current, include) => current.Include(include));

    // Apply ordering
    if (specification.OrderBy != null)
    {
      query = query.OrderBy(specification.OrderBy);
    }
    else if (specification.OrderByDescending != null)
    {
      query = query.OrderByDescending(specification.OrderByDescending);
    }

    // Apply paging
    if (specification.IsPagingEnabled)
    {
      query = query.Skip(specification.Skip).Take(specification.Take);
    }

    return query;
  }
}