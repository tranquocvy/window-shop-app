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
        // // Step 1: Build base query with Include for Category (eager loading)
        // var query = _context.Products
        //     .AsQueryable();

        // // Step 2: Apply filters
        // query = ApplyFilters(query, criteria.SearchTerm, criteria.IsDraft);

        // // Step 3: Get total count BEFORE pagination (this executes a COUNT query on DB)
        // var totalCount = await query.CountAsync(cancellationToken);

        // // Step 4: Apply sorting
        // query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

        // // Step 5: Apply pagination (Skip and Take are translated to OFFSET and LIMIT in SQL)
        // var items = await query
        //     .Skip((criteria.PageNumber - 1) * criteria.PageSize)
        //     .Take(criteria.PageSize)
        //     .AsNoTracking() // Performance optimization: don't track entities
        //     .ToListAsync(cancellationToken);

        // return (items, totalCount);

        var spec = new ProductSearchSpecification(criteria);

        _logger.LogInformation(
            "Searching products with criteria {@Criteria}",
            criteria);

        var items = await GetAsync(spec, cancellationToken);

        // Count total (không paging)
        var countSpec = new ProductSearchSpecification(criteria.SearchTerm, criteria.IsDraft);
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

    public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
  {
    return await ExecuteOperationAsync(
        "GetTotalProductCount",
        () => _dbSet
            .Where(p => p.IsDraft == false)
            .CountAsync(cancellationToken));
  }
}