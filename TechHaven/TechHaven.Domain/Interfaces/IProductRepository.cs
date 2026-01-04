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

        /// <summary>
        /// Get products with out of stock
        /// </summary>
        Task<IReadOnlyList<Product>>
        GetOutOfStockAsync(
            CancellationToken cancellationToken = default
        );

        // INTERFACE REPOSITORY FOR DASHBOARD

        /// <summary>
        /// Đếm tổng số sản phẩm (không tính draft)
        /// </summary>
        Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default);

        // INTERFACE REPOSITORY FOR BRAND
        Task<List<BrandInfo>> GetBrandsAsync(
            string searchTerm,
            bool? inStockOnly,
            string? sortBy,
            bool sortDescending = false,
            CancellationToken cancellationToken = default
        );
    }

    public class BrandInfo
    {
        /// <summary>
        /// Brand name (unique)
        /// </summary>
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Number of products under this brand
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Lowest price among products of this brand
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Highest price among products of this brand
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Total stock quantity for all products of this brand
        /// </summary>
        public int TotalStock { get; set; }
    }
}