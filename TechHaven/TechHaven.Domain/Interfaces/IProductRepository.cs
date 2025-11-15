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
        /// <summary>
        /// Build query for searching products.
        /// Does NOT execute SQL. Returning IQueryable allows
        /// higher layers (Application) to apply paging and sorting.
        /// </summary>
        IQueryable<Product> Search(
            string? searchTerm = null,
            bool? isDraft = null);

        /// <summary>
        /// Build query for low-stock products. Also NOT executed.
        /// Threshold = 0 returns out-of-stock products.
        /// </summary>
        IQueryable<Product> GetLowStock(int threshold = 0);
    }
}
