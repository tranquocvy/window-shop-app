using System.Linq.Expressions;
using System.Reflection;

namespace TechHaven.Application.Common.Helper;

public static class SortingHelper
{
    public static IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        string? sortBy,
        bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        // Tìm property phù hợp (case-insensitive)
        var property = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Equals(sortBy, StringComparison.OrdinalIgnoreCase));

        if (property == null)
            return query;

        // Tạo expression x => x.Property
        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(orderByExpression));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    // Optional: fallback cho IEnumerable
    public static IEnumerable<T> ApplySortingEnumerable<T>(
        IEnumerable<T> items,
        string? sortBy,
        bool descending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return items;

        var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return items;

        return descending
            ? items.OrderByDescending(x => property.GetValue(x))
            : items.OrderBy(x => property.GetValue(x));
    }
}
