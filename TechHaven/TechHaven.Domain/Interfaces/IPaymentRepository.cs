using System;
using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IReadOnlyList<Payment>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

        Task<decimal> GetTotalPaidForOrderAsync(int orderId, CancellationToken cancellationToken = default);
    }
}