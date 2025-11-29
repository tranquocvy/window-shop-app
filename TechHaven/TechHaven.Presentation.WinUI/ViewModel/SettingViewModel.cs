using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Shared.DTOs.AppSettings;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class SettingViewModel : ObservableObject
    {
        private readonly IAppSettingService _settingService;

        public SettingViewModel() : this(new MockSettingService()) { }

        public SettingViewModel(IAppSettingService settingService)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        [ObservableProperty]
        private string? currentTheme = null;

        [ObservableProperty]
        private int pageSize = 20;

        // LastVisitedPage implemented explicitly to avoid source-generator ordering issues
        public string? LastVisitedPage { get; private set; }

        public async Task InitializeAsync()
        {
            var dto = await _settingService.GetByKeyAsync("ThemeMode");
            if (dto?.Value != null)
            {
                CurrentTheme = dto.Value;

                if (Enum.TryParse<ThemeManager.ThemeType>(dto.Value, true, out var parsed))
                {
                    ThemeManager.ApplyTheme(parsed);
                }
            }
            else
            {
                CurrentTheme = null;
            }

            var pDto = await _settingService.GetByKeyAsync("PageSize");
            if (pDto?.Value != null && int.TryParse(pDto.Value, out var parsedSize))
            {
                PageSize = parsedSize;
            }

            var lDto = await _settingService.GetByKeyAsync("LastVisitedPage");
            if (lDto?.Value != null)
            {
                LastVisitedPage = lDto.Value;
            }
        }

        public async Task<bool> SetThemeAsync(string selected)
        {
            var dto = new AppSettingUpsertRequestDto
            {
                Value = selected,
                ValueType = SettingType.String,
                Category = "UI",
                Description = "User-selected color mode",
                IsSystem = false
            };

            var ok = await _settingService.UpsertAsync("ThemeMode", dto);
            if (!ok) return false;

            CurrentTheme = selected;
            return true;
        }

        public async Task<bool> SetPageSizeAsync(int size)
        {
            var dto = new AppSettingUpsertRequestDto
            {
                Value = size.ToString(),
                ValueType = SettingType.Number,
                Category = "UI",
                Description = "User-selected page size",
                IsSystem = false
            };

            var ok = await _settingService.UpsertAsync("PageSize", dto);
            if (!ok) return false;

            PageSize = size;
            return true;
        }

        public async Task<bool> SetLastVisitedPageAsync(string? tag)
        {
            var dto = new AppSettingUpsertRequestDto
            {
                Value = tag,
                ValueType = SettingType.String,
                Category = "UI",
                Description = "Last visited page tag",
                IsSystem = false
            };

            var ok = await _settingService.UpsertAsync("LastVisitedPage", dto);
            if (!ok) return false;

            LastVisitedPage = tag;
            return true;
        }
    }
}
