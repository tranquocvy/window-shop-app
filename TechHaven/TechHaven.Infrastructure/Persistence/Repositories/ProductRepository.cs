using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Search products with pagination, filtering, and sorting applied on database level
    /// This method is optimized to avoid loading all data into memory
    /// </summary>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchWithPaginationAsync(
        string? searchTerm = null,
        bool? isDraft = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Build base query with Include for Category (eager loading)
        var query = _context.Products
            .AsQueryable();

        // Step 2: Apply filters
        query = ApplyFilters(query, searchTerm, isDraft);

        // Step 3: Get total count BEFORE pagination (this executes a COUNT query on DB)
        var totalCount = await query.CountAsync(cancellationToken);

        // Step 4: Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        // Step 5: Apply pagination (Skip and Take are translated to OFFSET and LIMIT in SQL)
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking() // Performance optimization: don't track entities
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Apply filters to the query (returns IQueryable for further composition)
    /// </summary>
    private IQueryable<Product> ApplyFilters(
        IQueryable<Product> query,
        string? searchTerm,
        bool? isDraft)
    {
        // Filter by search term (ProductName or BrandName)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.ProductName.ToLower().Contains(term) ||
                p.BrandName.ToLower().Contains(term));
        }

        // Filter by draft status
        if (isDraft.HasValue)
        {
            query = query.Where(p => p.IsDraft == isDraft.Value);
        }

        return query;
    }

    /// <summary>
    /// Apply sorting to the query (returns IQueryable for further composition)
    /// </summary>
    private IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            // Default sorting
            return sortDescending
                ? query.OrderByDescending(p => p.ProductName)
                : query.OrderBy(p => p.ProductName);
        }

        // Apply specific sorting based on sortBy parameter
        return sortBy.ToLower() switch
        {
            "name" or "productname" => sortDescending
                ? query.OrderByDescending(p => p.ProductName)
                : query.OrderBy(p => p.ProductName),

            "price" or "sellprice" => sortDescending
                ? query.OrderByDescending(p => p.SellPrice)
                : query.OrderBy(p => p.SellPrice),

            "stock" or "stockquantity" => sortDescending
                ? query.OrderByDescending(p => p.StockQuantity)
                : query.OrderBy(p => p.StockQuantity),

            "brand" or "brandname" => sortDescending
                ? query.OrderByDescending(p => p.BrandName)
                : query.OrderBy(p => p.BrandName),

            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),

            "updatedat" => sortDescending
                ? query.OrderByDescending(p => p.UpdatedAt)
                : query.OrderBy(p => p.UpdatedAt),

            _ => sortDescending
                ? query.OrderByDescending(p => p.ProductName)
                : query.OrderBy(p => p.ProductName)
        };
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    public async Task<IReadOnlyList<Product>> GetLowStockAsync(
        int threshold = 0,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.StockQuantity <= threshold && !p.IsDraft)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}