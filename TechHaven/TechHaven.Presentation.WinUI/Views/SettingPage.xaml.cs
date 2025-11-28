using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using System;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class SettingPage : Page
    {
        private readonly SettingViewModel _viewModel;

        public SettingPage()
        {
            this.InitializeComponent();

            // Register this page root so ThemeManager can apply brushes to named elements
            ThemeManager.RegisterRoot(this);

            _viewModel = new SettingViewModel();

            LoadCurrentUser();

            // Fire-and-forget initialization to load persisted theme
            _ = InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            try
            {
                await _viewModel.InitializeAsync();

                // Apply persisted theme if valid
                if (!string.IsNullOrWhiteSpace(_viewModel.CurrentTheme) && Enum.TryParse<ThemeManager.ThemeType>(_viewModel.CurrentTheme, true, out var parsed))
                {
                    ThemeManager.ApplyTheme(parsed);
                }

                UpdateThemeStatus();
            }
            catch
            {
                // ignore init errors for now
                UpdateThemeStatus();
            }
        }

        private void LoadCurrentUser()
        {
            var user = AppState.CurrentUser;
            if (user != null)
            {
                currentUserFullNameText.Text = string.IsNullOrWhiteSpace(user.UserFullName) ? user.UserName : user.UserFullName;
                currentUserRoleText.Text = user.RoleName ?? "-";
            }
            else
            {
                currentUserFullNameText.Text = "Not signed in";
                currentUserRoleText.Text = "-";
            }
        }

        private void UpdateThemeStatus()
        {
            themeStatusText.Text = $"Current: {_viewModel.CurrentTheme ?? ThemeManager.CurrentTheme.ToString()}";
            themeButton.Content = $"Theme: {_viewModel.CurrentTheme ?? ThemeManager.CurrentTheme.ToString()}";
        }

        private async void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a menu list of available themes for the user to choose from
            var flyout = new MenuFlyout();

            foreach (ThemeManager.ThemeType t in Enum.GetValues(typeof(ThemeManager.ThemeType)))
            {
                var item = new MenuFlyoutItem { Text = t.ToString() };
                var theme = t; // capture
                item.Click += async (_, _) =>
                {
                    // Persist via ViewModel (which calls the setting service)
                    var ok = await _viewModel.SetThemeAsync(theme.ToString());
                    if (ok)
                    {
                        ThemeManager.ApplyTheme(theme);
                        UpdateThemeStatus();
                    }
                    else
                    {
                        // show a simple dialog to inform failure
                        var dlg = new ContentDialog
                        {
                            Title = "Save failed",
                            Content = "Unable to save theme setting.",
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot
                        };

                        await dlg.ShowAsync();
                    }
                };

                flyout.Items.Add(item);
            }

            // Show the flyout anchored to the button
            flyout.ShowAt(themeButton);
        }
    }
}
