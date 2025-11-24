using System.Linq;
using TechHaven.Domain.Entities;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Interfaces
{
    /// <summary>
    /// Product-specific repository supporting IQueryable for
    /// server-side filtering, sorting, and paging.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>
        /// Search products with pagination, filtering, and sorting applied on database level
        /// </summary>
        Task<(IReadOnlyList<Product> Items, int TotalCount)>
        SearchWithPaginationAsync(
            ProductSearchCriteria criteria,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get products with low stock
        /// </summary>
        Task<IReadOnlyList<Product>>
        GetLowStockAsync(
            int threshold = 0,
            CancellationToken cancellationToken = default
        );
    }
}