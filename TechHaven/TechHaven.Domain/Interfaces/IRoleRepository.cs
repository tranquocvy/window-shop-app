using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
    }
}