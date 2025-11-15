using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Product entity with search and filtering capabilities.
/// </summary>
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string? searchTerm = null,
        bool? isDraft = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Filter by search term (product name or brand)
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

        return await query
            .OrderBy(p => p.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold = 0, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= threshold && !p.IsDraft)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Product?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ProductId == (int)id, cancellationToken);
    }
}