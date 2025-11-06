using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        // Tìm user để đăng nhập
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        // Lấy user kèm Role (phân quyền)
        Task<IReadOnlyList<User>> GetWithRoleAsync(CancellationToken cancellationToken = default);

        // Lấy user theo role
        Task<IReadOnlyList<User>> GetByRoleAsync(int roleId, CancellationToken cancellationToken = default);

        // Lấy user active (IsActive = true)
        Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    }
}