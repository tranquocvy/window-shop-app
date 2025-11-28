using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Enums;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;
using TechHaven.Domain.Specifications;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Customer entity with search capabilities.
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<Customer?> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByPhone",
            () => _dbSet.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken),
            new { phoneNumber });
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "Search",
            async () =>
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
            },
            new { searchTerm });
    }

    public async Task<(IReadOnlyList<Customer> Items, int totalCount)> SearchWithPaginationAsync(CustomerSearchCriteria criteria, CancellationToken cancellationToken)
    {
        // var query = _context.Customers.AsQueryable();

        // query = ApplyFilters(query, criteria.SearchTerm);

        // var totalCount = await query.CountAsync(cancellationToken);

        // query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

        // var items = await query
        //     .Skip((criteria.PageNumber - 1) * criteria.PageSize)
        //     .Take(criteria.PageSize)
        //     .AsNoTracking()
        //     .ToListAsync(cancellationToken);

        // return (items, totalCount);

        // Get customer items with paging
        _logger.LogInformation(
            "Searching customers with pagination. Term: {SearchTerm}, Page: {Page}/{Size}",
            criteria.SearchTerm,
            criteria.PageNumber,
            criteria.PageSize);

        var spec = new CustomerSearchSpecification(criteria);
        var items = await GetAsync(spec, cancellationToken);

        // Get totalCount customers without paging (getAll Customers)
        var countSpec = new CustomerSearchSpecification(criteria.SearchTerm);
        var totalCount = await CountAsync(countSpec, cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Customer>> GetWithOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetWithOrders",
            () => _dbSet
                .Include(c => c.Orders)
                .OrderByDescending(c => c.TotalPurchased)
                .ToListAsync(cancellationToken));
    }

    public async Task<Customer?> GetOrdersByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetOrdersByCustomerId",
            () => _dbSet
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken),
            new { customerId });
    }

    public async Task<IReadOnlyList<Customer>> GetVipCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetVipCustomers",
            () => _dbSet
                .Where(c => c.Type == CustomerType.VIP)
                .OrderByDescending(c => c.TotalPurchased)
                .ToListAsync(cancellationToken));
    }
    // private IQueryable<Customer> ApplySorting(IQueryable<Customer> query, string? sortBy, bool sortDescending)
    // {
    //     if (string.IsNullOrWhiteSpace(sortBy))
    //     {
    //         return sortDescending
    //             ? query.OrderByDescending(p => p.CustomerName)
    //             : query.OrderBy(p => p.CustomerName);
    //     }

    //     return sortBy.ToLower() switch
    //     {
    //         "name" or "customername" => sortDescending
    //             ? query.OrderByDescending(p => p.CustomerName)
    //             : query.OrderBy(p => p.CustomerName),

    //         "address" => sortDescending
    //             ? query.OrderByDescending(p => p.Address)
    //             : query.OrderBy(p => p.Address),

    //         "type" or "customertype" => sortDescending
    //             ? query.OrderByDescending(p => p.Type)
    //             : query.OrderBy(p => p.Type),
            
    //         "totalpurchased" => sortDescending
    //             ? query.OrderByDescending(p => p.TotalPurchased)
    //             : query.OrderBy(p => p.TotalPurchased),
            
    //         _ => sortDescending
    //             ? query.OrderByDescending(p => p.CustomerName)
    //             : query.OrderBy(p => p.CustomerName)
    //     };
    // }

    // private IQueryable<Customer> ApplyFilters(IQueryable<Customer> query, string? searchTerm)
    // {
    //     if (!string.IsNullOrWhiteSpace(searchTerm))
    //     {
    //         var term = searchTerm.ToLower();
    //         query = query.Where(p =>
    //             p.CustomerName.ToLower().Contains(term)
    //         );
    //     }

    //     return query;
    // }
}