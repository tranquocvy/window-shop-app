using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Customer entity with search capabilities.
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.CustomerName.ToLower().Contains(term) ||
                c.PhoneNumber.Contains(term) ||
                (c.Email != null && c.Email.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(c => c.CustomerName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetWithOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Orders)
            .OrderByDescending(c => c.TotalPurchased)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetVipCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Type == CustomerType.VIP)
            .OrderByDescending(c => c.TotalPurchased)
            .ToListAsync(cancellationToken);
    }
}