using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using System.Diagnostics;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        // App version constant - update this value to change shown version
        private const string AppVersion = "v1.0.1";

        public MainWindow()
        {
            this.InitializeComponent();
            _viewModel = new MainWindowViewModel();

            // Set DataContext on root element so XAML {Binding} works
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = _viewModel;
            }

            // Use explicit app version constant
            try
            {
                versionTextBlock.Text = AppVersion;
            }
            catch { }
        }

        // Phương thức xử lý sự kiện Click
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // Require API to be explicitly configured via Settings
            if (!AppState.IsApiConfigured)
            {
                var prompt = new ContentDialog
                {
                    Title = "Server not configured",
                    Content = "Please configure the server URL in Settings before logging in.",
                    PrimaryButtonText = "Open Settings",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.Content.XamlRoot
                };

                var r = await prompt.ShowAsync();
                if (r == ContentDialogResult.Primary)
                {
                    await ShowSettingsDialogAsync();
                }

                // If still not configured, abort login
                if (!AppState.IsApiConfigured)
                {
                    return;
                }
            }

            // Set username and password from UI to ViewModel
            _viewModel.Username = usernameBox.Text;
            _viewModel.Password = passwordBox.Password;

            // Execute login command
            await _viewModel.LoginCommand.ExecuteAsync(null);

            // Check for errors (ViewModel.ErrorMessage is bound to the TextBlock in XAML)
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                return;
            }

            // Check if OTP is required
            if (_viewModel.RequiresOtp)
            {
                var otpOk = await ShowOtpDialogAndVerifyAsync();
                if (!otpOk)
                {
                    return;
                }
            }

            // Complete login and get user info
            var success = await _viewModel.CompleteLoginAndGetUserAsync();
            if (!success)
            {
                return;
            }

            // Persist last visited page setting immediately after login for debugging
            try
            {
                var settingsVm = new SettingViewModel();
                var ok = await settingsVm.SetLastVisitedPageAsync("shell");
                Debug.WriteLine($"SetLastVisitedPageAsync called after login -> success={ok}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error calling SetLastVisitedPageAsync after login: {ex}");
            }

            try { ApiClientFactory.ResetClient(); } catch { }

            // Navigate to ShellWindow
            var shellWindow = new ShellWindow();

            // [BẮT BUỘC] Cập nhật biến Static để Picker hoạt động
            TechHaven.Presentation.WinUI.App.MainWindow = shellWindow;
            App.MainWindow = shellWindow;
            shellWindow.Activate();

            _ = shellWindow.TriggerTrialCheckAsync();

            // Close login window
            this.Close();
        }

        // Settings text tapped (clickable text under the login button)
        private async void configTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await ShowSettingsDialogAsync();
        }

        // Show settings dialog and apply new API URL if saved. Returns true if saved/applied.
        private async Task<bool> ShowSettingsDialogAsync()
        {
            // Create UI for settings dialog
            var urlBox = new TextBox
            {
                Text = AppState.ApiBaseUri?.ToString() ?? string.Empty,
                PlaceholderText = "http://localhost:5207/",
                Width = 360,
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6)
            };

            var info = new TextBlock { Text = "Configure server base URL:", Margin = new Thickness(0, 0, 0, 6) };

            var stack = new StackPanel { Spacing = 8 };
            stack.Children.Add(info);
            stack.Children.Add(urlBox);

            var dialog = new ContentDialog
            {
                Title = "Settings",
                Content = stack,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var newUrl = urlBox.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(newUrl))
                {
                    AppState.SetApiBaseUri(newUrl);
                    ApiClientFactory.ResetClient();

                    // Recreate ViewModel with new HttpAuthService using updated HttpClient
                    _viewModel = new MainWindowViewModel(
                        new HttpAuthService(ApiClientFactory.GetHttpClient()),
                        new HttpUserService(ApiClientFactory.GetHttpClient())
                    );

                    // Rebind DataContext so bindings still work
                    if (this.Content is FrameworkElement root)
                    {
                        root.DataContext = _viewModel;
                    }

                    // Optionally show a brief confirmation
                    var confirmation = new ContentDialog
                    {
                        Title = "Settings saved",
                        Content = $"Server URL set to: {AppState.ApiBaseUri}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await confirmation.ShowAsync();

                    return true;
                }
            }

            return false;
        }

        private static string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return $"Thời gian còn lại: {ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private async Task<bool> ShowOtpDialogAndVerifyAsync()
        {
            // Create UI elements
            var otpBox = new TextBox { PlaceholderText = "Enter OTP", MaxLength = 6, Width = 220 };
            var infoText = new TextBlock { Text = _viewModel.OtpInfo, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) };
            var countdownText = new TextBlock { Text = FormatTime(_viewModel.OtpRemaining), Margin = new Thickness(0, 0, 0, 6) };

            var errorText = new TextBlock { Text = string.Empty, Foreground = new SolidColorBrush(Colors.Red), Margin = new Thickness(0, 6, 0, 0) };

            // Resolve theme brushes (fallbacks if resource not found)
            Brush accentBrush = new SolidColorBrush(Colors.CornflowerBlue);
            Brush accentForeground = new SolidColorBrush(Colors.White);
            Brush subtleBrush = new SolidColorBrush(Colors.Transparent);

            if (Application.Current?.Resources != null)
            {
                if (Application.Current.Resources.ContainsKey("SystemControlBackgroundAccentBrush"))
                    accentBrush = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as Brush ?? accentBrush;
                if (Application.Current.Resources.ContainsKey("SystemControlForegroundBaseHighBrush"))
                    accentForeground = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as Brush ?? accentForeground;
                if (Application.Current.Resources.ContainsKey("SystemControlBackgroundBaseLowBrush"))
                    subtleBrush = Application.Current.Resources["SystemControlBackgroundBaseLowBrush"] as Brush ?? subtleBrush;
            }

            var verifyButton = new Button
            {
                Content = "Verify",
                IsEnabled = _viewModel.IsVerifyEnabled,
                Width = 110,
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(6),
                Background = accentBrush,
                Foreground = accentForeground
            };

            var resendButton = new Button
            {
                Content = "Resend",
                Width = 110,
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(6),
                Background = subtleBrush,
                Foreground = accentBrush
            };

            var closeButton = new Button
            {
                Content = "Close",
                Width = 110,
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(6),
                Background = subtleBrush,
                Foreground = new SolidColorBrush(Colors.Gray)
            };

            // Spacing and alignment
            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0), Spacing = 12 };
            buttonsPanel.Children.Add(verifyButton);
            buttonsPanel.Children.Add(resendButton);
            buttonsPanel.Children.Add(closeButton);

            var stack = new StackPanel();
            stack.Children.Add(infoText);
            stack.Children.Add(countdownText);
            stack.Children.Add(new TextBlock { Text = "", Height = 6 });
            stack.Children.Add(otpBox);
            stack.Children.Add(errorText);
            stack.Children.Add(buttonsPanel);

            var dialog = new ContentDialog
            {
                Title = "Xác thực OTP",
                Content = stack,
                XamlRoot = this.Content.XamlRoot
            };

            // Hover effects: change background on pointer enter/exit
            void AttachHover(Button btn, Brush normal, Brush hover)
            {
                btn.PointerEntered += (_, _) => btn.Background = hover ?? normal;
                btn.PointerExited += (_, _) => btn.Background = normal;
            }

            // compute hover brushes (slightly darker/lighter)
            Brush hoverAccent = accentBrush;
            Brush hoverSubtle = subtleBrush;
            try
            {
                // if accent is SolidColorBrush, change opacity to simulate hover
                if (accentBrush is SolidColorBrush s1)
                    hoverAccent = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, (byte)Math.Min(255, s1.Color.R + 10), (byte)Math.Min(255, s1.Color.G + 10), (byte)Math.Min(255, s1.Color.B + 10)));
                if (subtleBrush is SolidColorBrush s2)
                    hoverSubtle = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, (byte)Math.Min(255, s2.Color.R + 8), (byte)Math.Min(255, s2.Color.G + 8), (byte)Math.Min(255, s2.Color.B + 8)));
            }
            catch { /* ignore color math issues */ }

            AttachHover(verifyButton, accentBrush, hoverAccent);
            AttachHover(resendButton, subtleBrush, hoverSubtle);
            AttachHover(closeButton, subtleBrush, hoverSubtle);

            // Timer to update countdown and verify button state
            var remaining = _viewModel.OtpRemaining;
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (s, e) =>
            {
                remaining = Math.Max(0, remaining - 1);
                var text = FormatTime(remaining);
                _ = this.DispatcherQueue.TryEnqueue(() =>
                {
                    countdownText.Text = text;
                    verifyButton.IsEnabled = remaining > 0;
                });
            };

            timer.Start();

            bool verified = false;
            bool closed = false;

            // Handlers
            verifyButton.Click += async (_, _) =>
            {
                verifyButton.IsEnabled = false;
                _viewModel.OtpInput = otpBox.Text?.Trim() ?? string.Empty;
                await _viewModel.VerifyOtpCommand.ExecuteAsync(null);
                if (!_viewModel.RequiresOtp)
                {
                    verified = true;
                    dialog.Hide();
                }
                else
                {
                    errorText.Text = _viewModel.ErrorMessage ?? "Invalid OTP.";
                    _ = this.DispatcherQueue.TryEnqueue(() => verifyButton.IsEnabled = remaining > 0);
                }
            };

            resendButton.Click += async (_, _) =>
            {
                await _viewModel.ResendOtpCommand.ExecuteAsync(null);
                remaining = _viewModel.OtpRemaining;
                _ = this.DispatcherQueue.TryEnqueue(() =>
            {
                    otpBox.Text = string.Empty;
                    errorText.Text = string.Empty;
                    countdownText.Text = FormatTime(_viewModel.OtpRemaining);
                    verifyButton.IsEnabled = remaining > 0;
                });
            };

            closeButton.Click += (_, _) =>
            {
                closed = true;
                dialog.Hide();
            };

            // Show dialog
            await dialog.ShowAsync();

            // Stop timer
            timer.Stop();
            timer.Dispose();

            if (verified)
                return true;

            if (closed || remaining <= 0)
            {
                if (remaining <= 0)
                    _viewModel.ErrorMessage = "OTP expired.";

                return false;
            }

            return false;
        }
    }
}
