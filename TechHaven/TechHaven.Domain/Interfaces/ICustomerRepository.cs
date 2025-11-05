using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Customer>> SearchAsync(
            string? searchTerm = null,       // Từ khóa tìm kiếm (tên, số điện thoại, v.v.)
            CancellationToken cancellationToken = default);
            
        // Lấy khách hàng kèm đơn hàng (Transaction data)
        Task<IReadOnlyList<Customer>> GetWithOrdersAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Customer>> GetVipCustomersAsync(CancellationToken cancellationToken = default);
    }
}