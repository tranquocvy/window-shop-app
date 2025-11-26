using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
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
        private string currentTheme = "Light";

        public async Task InitializeAsync()
        {
            var dto = await _settingService.GetByKeyAsync("ThemeMode");
            CurrentTheme = dto?.Value ?? "Light";
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
    }
}
