using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Views;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockSettingService : IAppSettingService
    {
        private readonly ConcurrentDictionary<string, AppSettingDto> _store = new();

        public MockSettingService()
        {
            _store["ThemeMode"] = new AppSettingDto
            {
                AppSettingId = 1,
                Key = "ThemeMode",
                Value = "Midnight",
                ValueType = SettingType.String,
                Category = "UI",
                Description = "User-selected color mode",
                IsSystem = false,
                UpdatedAt = DateTime.UtcNow
            };

            _store["PageSize"] = new AppSettingDto
            {
                AppSettingId = 2,
                Key = "PageSize",
                Value = "20",
                ValueType = SettingType.Number,
                Category = "UI",
                Description = "Number of items per page",
                IsSystem = false,
                UpdatedAt = DateTime.UtcNow
            };

            _store["LastVisitedPage"] = new AppSettingDto
            {
                AppSettingId = 3,
                Key = "LastVisitedPage",
                Value = "setting",
                ValueType = SettingType.String,
                Category = "UI",
                Description = "Last visited page tag",
                IsSystem = false,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public Task<AppSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key != null && _store.TryGetValue(key, out var dto))
                return Task.FromResult<AppSettingDto?>(dto);
            return Task.FromResult<AppSettingDto?>(null);
        }

        public Task<bool> UpsertAsync(string key, AppSettingUpsertRequestDto dto, CancellationToken cancellationToken = default)
        {
            if (key == null) return Task.FromResult(false);

            var entry = _store.GetOrAdd(key, _ => new AppSettingDto { Key = key });
            entry.Key = key;
            entry.Value = dto.Value;
            entry.ValueType = dto.ValueType;
            entry.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
    }
}
