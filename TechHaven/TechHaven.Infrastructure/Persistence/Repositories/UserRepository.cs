using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User entity with specific business queries.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByUserName",
            () => _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken),
            new { userName });
    }

    public async Task<IReadOnlyList<User>> GetWithRoleAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetWithRole",
            () => _dbSet
                .Include(u => u.Role)
                .ToListAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByRole",
            () => _dbSet
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId)
                .ToListAsync(cancellationToken),
            new { roleId });
    }

    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetActiveUsers",
            () => _dbSet
                .Include(u => u.Role)
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken));
    }

    public override async Task<User?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByIdWithRole",
            () => _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == (int)id, cancellationToken),
            new { id });
    }
}