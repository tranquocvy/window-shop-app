using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Presentation.WinUI.Services.Interfaces
{
    public interface IAppSettingService
    {
        Task<AppSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> UpsertAsync(string key, AppSettingUpsertRequestDto dto, CancellationToken cancellationToken = default);
    }
}
