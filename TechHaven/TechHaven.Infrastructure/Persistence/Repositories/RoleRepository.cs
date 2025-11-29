using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Role entity.
/// </summary>
public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByName",
            () => _dbSet.FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken),
            new { roleName });
    }
}