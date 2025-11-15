using Microsoft.EntityFrameworkCore;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AppSetting entity.
/// </summary>
public class AppSettingRepository : GenericRepository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<AppSetting>> GetByPrefixAsync(
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Key.StartsWith(keyPrefix))
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }
}