using System.Collections.Generic;
using System.Threading;
using TechHaven.Domain.Entities;

namespace TechHaven.Domain.Interfaces
{
    public interface IAppSettingRepository : IGenericRepository<AppSetting>
    {
        Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AppSetting>> GetByPrefixAsync(string keyPrefix, CancellationToken cancellationToken = default);
    }
}