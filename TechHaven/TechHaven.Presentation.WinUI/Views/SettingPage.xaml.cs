using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using System;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class SettingPage : Page
    {
        private readonly SettingViewModel _viewModel;
        private bool _isInitializing = false;

        public SettingPage()
        {
            this.InitializeComponent();

            // Register this page root so ThemeManager can apply brushes to named elements
            ThemeManager.RegisterRoot(this);

            _viewModel = new SettingViewModel();

            LoadCurrentUser();

            UpdateThemeStatus();

            ThemeManager.ThemeChanged += OnThemeChanged;
            this.Unloaded += SettingPage_Unloaded;

            // Populate page size options and load persisted settings
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            _isInitializing = true;

            // populate combo box options
            pageSizeCombo.ItemsSource = new int[] { 5, 10, 20, 50 };

            // load persisted settings from view model
            try
            {
                await _viewModel.InitializeAsync();

                // set selected page size
                pageSizeCombo.SelectedItem = _viewModel.PageSize;

                // update theme status if viewmodel provided theme
                UpdateThemeStatus();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void SettingPage_Unloaded(object? sender, RoutedEventArgs e)
        {
            ThemeManager.ThemeChanged -= OnThemeChanged;
            this.Unloaded -= SettingPage_Unloaded;
        }

        private void LoadCurrentUser()
        {
            var user = AppState.CurrentUser;
            if (user != null)
            {
                currentUserFullNameText.Text = string.IsNullOrWhiteSpace(user.UserFullName) ? user.UserName : user.UserFullName;
                currentUserRoleText.Text = user.RoleName ?? "-";

                currentUserUserNameText.Text = user.UserName ?? string.Empty;
                currentUserEmailText.Text = user.Email ?? string.Empty;
            }
            else
            {
                currentUserFullNameText.Text = "Not signed in";
                currentUserRoleText.Text = "-";
                currentUserUserNameText.Text = string.Empty;
                currentUserEmailText.Text = string.Empty;
            }
        }

        private void UpdateThemeStatus()
        {
            // Prefer the viewmodel value if present (it may be null if we didn't initialize here), otherwise use ThemeManager's current theme
            var themeText = _viewModel.CurrentTheme ?? ThemeManager.CurrentTheme.ToString();
            themeStatusText.Text = $"Current: {themeText}";
            themeButton.Content = $"Theme: {themeText}";

            // Also apply current theme brushes to this page immediately
            ApplyThemeBrushes();
        }

        private void ApplyThemeBrushes()
        {
            var appRes = Application.Current?.Resources;
            if (appRes == null) return;

            // Helper to safely get brush
            Brush? GetBrush(string key) => appRes.ContainsKey(key) ? appRes[key] as Brush : null;

            // Page background
            var pageBg = GetBrush("TH.SurfaceBackground");
            if (pageBg != null)
            {
                if (this.Content is Panel panel)
                {
                    panel.Background = pageBg;
                }
                else if (this.Content is Control control)
                {
                    control.Background = pageBg;
                }
                else
                {
                    // attempt via reflection to set Background property if available
                    try
                    {
                        var root = this.Content as FrameworkElement;
                        var prop = root?.GetType().GetProperty("Background");
                        if (prop != null && prop.CanWrite && prop.PropertyType.IsAssignableFrom(typeof(Brush)))
                        {
                            prop.SetValue(root, pageBg);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            // Text colors
            var primary = GetBrush("TH.TextPrimary");
            var secondary = GetBrush("TH.TextSecondary");

            if (primary != null)
                currentUserFullNameText.Foreground = primary;
            if (secondary != null)
                currentUserRoleText.Foreground = secondary;

            // Also update the username and email fields
            if (secondary != null)
            {
                currentUserUserNameText.Foreground = secondary;
                currentUserEmailText.Foreground = secondary;
            }

            // Update title and color mode label so they change immediately
            if (primary != null)
            {
                try { titleText.Foreground = primary; } catch { }
                try { colorModeLabel.Foreground = primary; } catch { }
            }

            // Button accent/foreground
            var primaryBrush = GetBrush("TH.PrimaryBrush");
            if (primaryBrush != null)
                themeButton.Background = primaryBrush;

            // Style the page size combo to match theme
            if (primary != null)
            {
                try { pageSizeCombo.Foreground = primary; } catch { }
            }

            // Status text
            if (secondary != null)
                themeStatusText.Foreground = secondary;
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
                        ApplyThemeBrushes();
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

        private async void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (pageSizeCombo.SelectedItem is int selected)
            {
                // persist via viewmodel
                var ok = await _viewModel.SetPageSizeAsync(selected);
                if (!ok)
                {
                    // show simple dialog on failure
                    var dlg = new ContentDialog
                    {
                        Title = "Save failed",
                        Content = "Unable to save page size setting.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };

                    await dlg.ShowAsync();

                    // revert selection to current value
                    pageSizeCombo.SelectedItem = _viewModel.PageSize;
                }
            }
        }

        private void OnThemeChanged(ThemeManager.ThemeType obj)
        {
            _ = this.DispatcherQueue?.TryEnqueue(() =>
            {
                UpdateThemeStatus();
                ApplyThemeBrushes();
            });
        }
    }
}
