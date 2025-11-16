using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User entity with specific business queries.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetWithRoleAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .Where(u => u.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);
    }

    public override async Task<User?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == (int)id, cancellationToken);
    }
}