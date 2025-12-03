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
}