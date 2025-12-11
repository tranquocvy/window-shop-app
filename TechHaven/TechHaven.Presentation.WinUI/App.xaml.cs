using Microsoft.UI.Xaml;
using System;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Views;

// QuestPDF license types
using QuestPDF;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;


        // thuộc tính này để gọi cái MainWindow từ các chỗ khác
        public static Window MainWindow { get; set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Configure QuestPDF license to Community for non-production use
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            }
            catch
            {
                // ignore license assignment failures
            }

            // Default theme
            ThemeManager.ThemeType initialTheme = ThemeManager.ThemeType.Midnight;

            // Ensure token restore runs BEFORE loading user-specific settings so API calls include auth token
            TokenPersistence.UseMock = false;

            try
            {
                var restored = await TokenPersistence.TryRestoreSessionAsync();
                if (restored && AppState.IsLoggedIn)
                {
                    try
                    {
                        var vm = new SettingViewModel();
                        await vm.InitializeAsync();
                        if (!string.IsNullOrWhiteSpace(vm.CurrentTheme) &&
                            Enum.TryParse<ThemeManager.ThemeType>(vm.CurrentTheme, true, out var parsed))
                        {
                            initialTheme = parsed;
                        }
                    }
                    catch
                    {
                        // ignore and fall back to default
                    }
                }
                else
                {
                    // Not restored: attempt to load system default theme (no auth)
                    try
                    {
                        var vm = new SettingViewModel();
                        await vm.InitializeAsync();
                        if (!string.IsNullOrWhiteSpace(vm.CurrentTheme) &&
                            Enum.TryParse<ThemeManager.ThemeType>(vm.CurrentTheme, true, out var parsed))
                        {
                            initialTheme = parsed;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            catch
            {
                // ignore
            }

            // Initialize theme manager with the persisted or fallback theme
            try
            {
                ThemeManager.Initialize(initialTheme, loadAccents: true);
            }
            catch
            {
                // ignore theme init failures
            }

            try
            {
                var restored = await TokenPersistence.TryRestoreSessionAsync();
                if (restored && AppState.IsLoggedIn)
                {
                    // open shell directly
                    var shell = new ShellWindow();
                    MainWindow = shell;
                    shell.Activate();
                    return;
                }
            }
            catch
            {
                // ignore
            }

            _window = new MainWindow();
            MainWindow = _window;
            _window.Activate();
        }
    }
}
