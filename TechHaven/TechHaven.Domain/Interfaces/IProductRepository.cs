using System.Linq;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    /// <summary>
    /// Product-specific repository supporting IQueryable for
    /// server-side filtering, sorting, and paging.
    /// </summary>
    public interface IProductRepository : IGenericRepository<Product>
    {
        // /// <summary>
        // /// Build query for searching products.
        // /// Does NOT execute SQL. Returning IQueryable allows
        // /// higher layers (Application) to apply paging and sorting.
        // /// </summary>
        // IQueryable<Product> Search(
        //     string? searchTerm = null,
        //     bool? isDraft = null);

        // /// <summary>
        // /// Build query for low-stock products. Also NOT executed.
        // /// Threshold = 0 returns out-of-stock products.
        // /// </summary>
        // IQueryable<Product> GetLowStock(int threshold = 0);

        /// <summary>
        /// Search products with pagination, filtering, and sorting applied on database level
        /// </summary>
        Task<(IReadOnlyList<Product> Items, int TotalCount)>
        SearchWithPaginationAsync(
            string? searchTerm = null,
            bool? isDraft = null,
            int pageNumber = 1,
            int pageSize = 20,
            string? sortBy = null,
            bool sortDescending = false,
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