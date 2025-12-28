using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;
using TechHaven.Domain.SearchCriteria;

namespace TechHaven.Domain.Interfaces
{
    public interface IAppSettingRepository : IGenericRepository<AppSetting>
    {
        Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AppSetting>> GetByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);

        Task<AppSetting?> GetSettingAsync(string key, int? userId = null, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<AppSetting> Items, int TotalCount)> SearchWithPaginationAsync( AppSettingSearchCriteria criteria,
                                                                                            CancellationToken cancellationToken);
    }
}