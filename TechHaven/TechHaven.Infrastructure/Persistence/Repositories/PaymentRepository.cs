using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Payment entity.
/// </summary>
public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OrderId == orderId)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Order)
                .ThenInclude(o => o!.Customer)
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalPaidForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var total = await _dbSet
            .Where(p => p.OrderId == orderId)
            .SumAsync(p => p.Amount, cancellationToken);

        return total;
    }
}