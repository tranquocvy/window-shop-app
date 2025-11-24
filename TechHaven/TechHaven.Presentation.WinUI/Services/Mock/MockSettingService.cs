using System;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.AppSettings;
using TechHaven.Presentation.WinUI.Services.Interfaces;

namespace TechHaven.Presentation.WinUI.Services.Mock
{
    public class MockSettingService : IAppSettingService
    {
        private AppSettingDto? _stored;

        public MockSettingService()
        {
            _stored = new AppSettingDto
            {
                AppSettingId = 1,
                Key = "ThemeMode",
                Value = "Light",
                ValueType = SettingType.String,
                Category = "UI",
                Description = "User-selected color mode",
                IsSystem = false,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public Task<AppSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == _stored?.Key) return Task.FromResult<AppSettingDto?>(_stored);
            return Task.FromResult<AppSettingDto?>(null);
        }

        public Task<bool> UpsertAsync(string key, AppSettingCreateUpdateDto dto, CancellationToken cancellationToken = default)
        {
            if (_stored == null)
            {
                _stored = new AppSettingDto();
            }

            _stored.Key = key;
            _stored.Value = dto.Value;
            _stored.ValueType = dto.ValueType;
            _stored.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(true);
        }
    }
}
