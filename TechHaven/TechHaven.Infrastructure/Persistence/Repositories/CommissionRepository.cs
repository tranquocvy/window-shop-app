using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Commission entity with KPI calculations.
/// </summary>
public class CommissionRepository : GenericRepository<Commission>, ICommissionRepository
{
    public CommissionRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<Commission?> GetByUserAndMonthAsync(
        int userId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByUserAndMonth",
            () => _dbSet
                .Include(c => c.User)
                .FirstOrDefaultAsync(
                    c => c.UserId == userId && c.Month == month && c.Year == year,
                    cancellationToken),
            new { userId, month, year });
    }

    public async Task<IReadOnlyList<Commission>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByUser",
            () => _dbSet
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .ToListAsync(cancellationToken),
            new { userId });
    }

    public async Task<IReadOnlyList<Commission>> GetByMonthYearAsync(
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByMonthYear",
            () => _dbSet
                .Include(c => c.User)
                    .ThenInclude(u => u!.Role)
                .Where(c => c.Month == month && c.Year == year)
                .OrderByDescending(c => c.TotalSales)
                .ToListAsync(cancellationToken),
            new { month, year });
    }

    public async Task<decimal> GetTotalCommissionAsync(
        int userId,
        int startMonth,
        int startYear,
        int endMonth,
        int endYear,
        CancellationToken cancellationToken = default)
    {
        // Convert month/year to a comparable format
        var commissions = await ExecuteOperationAsync(
            "GetRangeForTotal",
            () => _dbSet
                .Where(c => c.UserId == userId)
                .Where(c =>
                    (c.Year > startYear || (c.Year == startYear && c.Month >= startMonth)) &&
                    (c.Year < endYear || (c.Year == endYear && c.Month <= endMonth)))
                .ToListAsync(cancellationToken),
            new { userId, startMonth, startYear, endMonth, endYear });

        // Calculate total commission amount (computed property)
        return commissions.Sum(c => c.CommissionAmount);
    }
}