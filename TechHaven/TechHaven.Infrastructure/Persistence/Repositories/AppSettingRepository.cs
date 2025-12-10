using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Supabase.Gotrue;
using TechHaven.Domain.Entities;
using TechHaven.Domain.Interfaces;
using TechHaven.Domain.SearchCriteria;

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
    //Paing 
    public async Task<(IReadOnlyList<AppSetting> Items, int TotalCount)> SearchWithPaginationAsync(
        AppSettingSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        return await ExecuteOperationAsync(
            "SearchWithPagination",
            async () =>
            {
                var query = _dbSet.AsNoTracking().AsQueryable();

                // Filter: Lấy System Setting HOẶC User Setting của user hiện tại
                // Nếu criteria.UserId có giá trị -> Lấy (System + Của User đó)
                if (criteria.UserId.HasValue)
                {
                    query = query.Where(s => s.UserId == null || s.UserId == criteria.UserId);
                }
                else
                {
                    query = query.Where(s => s.UserId == null);
                }

                // Search Keyword (Key hoặc Category)
                if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
                {
                    var term = criteria.SearchTerm.ToLower();
                    query = query.Where(s => s.Key.ToLower().Contains(term) ||
                                             (s.Category != null && s.Category.ToLower().Contains(term)));
                }
                var totalCount = await query.CountAsync(cancellationToken);

                // Paging & Sorting
                // Sort: System lên trước, User xuống sau (hoặc ngược lại), sau đó sort theo Key
                var items = await query
                    .OrderBy(s => s.Key)
                    .ThenBy(s => s.UserId) // Null (System) xếp trước
                    .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToListAsync(cancellationToken);

                return (items, totalCount);
            },
            new { criteria.UserId, criteria.SearchTerm, criteria.PageNumber });
    }

    /// <summary>
    /// Retrieves an application setting by its unique key, applying user-specific fallback logic.
    /// </summary>
    /// <param name="key">The unique key of the setting (e.g., "Display.ItemsPerPage").</param>
    /// <param name="userId">The ID of the currently logged-in user. Null if requesting a global (system) setting.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task<AppSetting?> GetSettingAsync(string key, int? userId = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteOperationAsync(
            "GetByKeyWithFallback",
            async () =>
            {
                // Ưu tiên tìm user setting trước
                if (userId.HasValue)
                {
                    var userSetting = await _dbSet
                        .Where(s => s.Key == key && s.UserId == userId)
                        .FirstOrDefaultAsync(cancellationToken);
                    
                    if (userSetting != null)
                    {
                        return userSetting;
                    }
                }

                // Nếu không có user setting, fallback về system setting
                var systemSetting = await _dbSet
                    .Where(s => s.Key == key && s.UserId == null)
                    .FirstOrDefaultAsync(cancellationToken);

                return systemSetting;
            },
            //Metadata for loggings
            new { Key = key, UserId = userId }
        );
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