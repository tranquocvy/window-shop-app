using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Specifications;

namespace TechHaven.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    /// <summary>
    /// Search products with pagination, filtering, and sorting applied on database level
    /// This method is optimized to avoid loading all data into memory
    /// </summary>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchWithPaginationAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var spec = new ProductSearchSpecification(criteria, true);

        _logger.LogInformation(
            "Searching products with criteria {@Criteria}",
            criteria);

        _logger.LogInformation(
            "Searching products with criteria {@Criteria}",
            criteria);

        var items = await GetAsync(spec, cancellationToken);

        // Count total (không paging)
        var countSpec = new ProductSearchSpecification(criteria, false);
        var totalCount = await CountAsync(countSpec, cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetLowStockAsync(
        int threshold = 0,
        CancellationToken cancellationToken = default)
    {
        var spec = new LowStockProductsSpecification(threshold);
        _logger.LogInformation("Fetching products with stock threshold {Threshold}", threshold);
        return await GetAsync(spec, cancellationToken);
    }

    /// <summary>
    /// Get products with out of stock
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetOutOfStockAsync(
        CancellationToken cancellationToken = default)
    {
        var spec = new OutOfStockProductsSpecification();
        _logger.LogInformation("Fetching products with out of stock");
        return await GetAsync(spec, cancellationToken);
    }

    public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetTotalProductCount",
            () => _dbSet
                .Where(p => p.IsDraft == false)
                .CountAsync(cancellationToken));
    }

    public async Task<List<BrandInfo>> GetBrandsAsync(
        string searchTerm,
        bool? inStockOnly,
        string? sortBy,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetBrandsAsync",
            async () =>
            {
                var query = _dbSet.AsQueryable();

                // Lọc theo searchTerm nếu có
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p => p.BrandName.ToLower().Contains(searchTerm.ToLower()));
                }

                // Lọc theo tồn kho nếu cần
                if (inStockOnly == true)
                {
                    query = query.Where(p => p.StockQuantity > 0);
                }

                var grouped = query
                    .GroupBy(p => p.BrandName)
                    .Select(g => new BrandInfo
                    {
                        BrandName = g.Key,
                        ProductCount = g.Count(),
                        MinPrice = g.Min(p => p.SellPrice),
                        MaxPrice = g.Max(p => p.SellPrice),
                        TotalStock = g.Sum(p => p.StockQuantity)
                    });

                // Sắp xếp nếu có yêu cầu
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "productcount":
                            grouped = sortDescending ? grouped.OrderByDescending(b => b.ProductCount) : grouped.OrderBy(b => b.ProductCount);
                            break;
                        case "minprice":
                            grouped = sortDescending ? grouped.OrderByDescending(b => b.MinPrice) : grouped.OrderBy(b => b.MinPrice);
                            break;
                        case "maxprice":
                            grouped = sortDescending ? grouped.OrderByDescending(b => b.MaxPrice) : grouped.OrderBy(b => b.MaxPrice);
                            break;
                        case "totalstock":
                            grouped = sortDescending ? grouped.OrderByDescending(b => b.TotalStock) : grouped.OrderBy(b => b.TotalStock);
                            break;
                        default:
                            grouped = sortDescending ? grouped.OrderByDescending(b => b.BrandName) : grouped.OrderBy(b => b.BrandName);
                            break;
                    }
                }

                return await grouped.ToListAsync(cancellationToken);
            }
        );
    }
}