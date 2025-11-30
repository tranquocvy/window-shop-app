using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;

namespace TechHaven.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for AppSetting entity.
/// </summary>
public class AppSettingRepository : GenericRepository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(AppDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
    {
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByKey",
            () => _dbSet.FirstOrDefaultAsync(s => s.Key == key, cancellationToken),
            new { Key = key });
    }

    public async Task<IReadOnlyList<AppSetting>> GetByPrefixAsync(
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByPrefix",
            () => _dbSet
                .Where(s => s.Key.StartsWith(keyPrefix))
                .OrderBy(s => s.Key)
                .ToListAsync(cancellationToken),
            new { Prefix = keyPrefix });
    }
}