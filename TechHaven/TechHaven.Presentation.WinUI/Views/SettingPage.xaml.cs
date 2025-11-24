using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TechHaven.Presentation.WinUI.ViewModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private readonly SettingViewModel _viewModel;

        public SettingPage()
        {
            this.InitializeComponent();

            _viewModel = new SettingViewModel();
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = _viewModel;
            }

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _viewModel.InitializeAsync();
            themeButton.Content = _viewModel.CurrentTheme;
            UpdateStatusText();

            // Apply preview
            ApplyPreviewTheme(_viewModel.CurrentTheme);

            // Also apply page theme
            this.RequestedTheme = string.Equals(_viewModel.CurrentTheme, "Dark", StringComparison.OrdinalIgnoreCase) ? ElementTheme.Dark : ElementTheme.Light;
        }

        private void UpdateStatusText()
        {
            themeStatusText.Text = $"Current: {themeButton.Content}";
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();
            var light = new MenuFlyoutItem { Text = "Light" };
            var dark = new MenuFlyoutItem { Text = "Dark" };

            light.Click += ThemeMenu_Click;
            dark.Click += ThemeMenu_Click;

            // Add check mark to indicate current selection
            if (themeButton.Content?.ToString() == "Light")
            {
                light.Icon = new FontIcon { Glyph = "\uE73E" }; // Check glyph
            }
            else if (themeButton.Content?.ToString() == "Dark")
            {
                dark.Icon = new FontIcon { Glyph = "\uE73E" };
            }

            flyout.Items.Add(light);
            flyout.Items.Add(dark);

            flyout.ShowAt(themeButton);
        }

        private async void ThemeMenu_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem item)
                {
                    var selected = item.Text;

                    var ok = await _viewModel.SetThemeAsync(selected);
                    if (ok)
                    {
                        themeButton.Content = _viewModel.CurrentTheme;
                        UpdateStatusText();

                        // update preview
                        ApplyPreviewTheme(selected);

                        // apply to page
                        this.RequestedTheme = string.Equals(selected, "Dark", StringComparison.OrdinalIgnoreCase) ? ElementTheme.Dark : ElementTheme.Light;

                        // apply to entire app
                        ApplyAppTheme(selected);
                    }
                    else
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "Error",
                            Content = "Failed to save theme.",
                            CloseButtonText = "Close"
                        };
                        _ = dialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Show error dialog with stack trace to aid debugging
                try
                {
                    var dlg = new ContentDialog
                    {
                        Title = "Theme change error",
                        Content = $"{ex.GetType().Name}: {ex.Message}\n\n{ex.StackTrace}",
                        CloseButtonText = "Close"
                    };
                    _ = dlg.ShowAsync();
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("ThemeMenu_Click exception: " + ex);
            }
        }

        private void ApplyPreviewTheme(string selected)
        {
            var isDark = string.Equals(selected, "Dark", StringComparison.OrdinalIgnoreCase);
            if (previewPanel != null)
            {
                previewPanel.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;
            }
        }

        private void ApplyAppTheme(string selected)
        {
            var isDark = string.Equals(selected, "Dark", StringComparison.OrdinalIgnoreCase);

            // Do NOT set Application.RequestedTheme due to COM issues on some systems.
            // Only update main window root and ShellWindow.ApplyTheme.

            // Apply to main window root safely
            try
            {
                var main = App.MainWindow;
                if (main == null)
                {
                    System.Diagnostics.Debug.WriteLine("App.MainWindow is null when applying theme.");
                    return;
                }

                if (main.Content is FrameworkElement root)
                {
                    var dq = main.DispatcherQueue;
                    if (dq != null)
                    {
                        dq.TryEnqueue(() =>
                        {
                            try
                            {
                                root.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error setting root.RequestedTheme: " + ex);
                            }

                            // If ShellWindow has ApplyTheme method, call it
                            try
                            {
                                if (main is Views.ShellWindow shell)
                                {
                                    shell.ApplyTheme(isDark ? ElementTheme.Dark : ElementTheme.Light);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("Error calling ShellWindow.ApplyTheme: " + ex);
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            root.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;

                            if (main is Views.ShellWindow shell)
                            {
                                shell.ApplyTheme(isDark ? ElementTheme.Dark : ElementTheme.Light);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error setting root.RequestedTheme (no dispatcher): " + ex);
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Main window content is not a FrameworkElement.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ApplyAppTheme exception: " + ex);
            }
        }
    }
}
