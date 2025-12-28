using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Auth;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Manages trial mode UI and activation logic
    /// </summary>
    public class TrialModeManager
    {
        private readonly IAuthService _authService;
        private readonly Window _window;
        private Grid? _overlayGrid;
        private bool _isActive = false;

        public bool IsTrialModeActive => _isActive;

        public TrialModeManager(Window window, IAuthService authService)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Check trial status and show blocking overlay if expired
        /// </summary>
        public async Task CheckAndShowTrialModeAsync()
        {
            try
            {
                if (AppState.CurrentUser == null || string.IsNullOrWhiteSpace(TokenStore.AccessToken))
                    return;

                var result = await _authService.CheckTrialStatusAsync();

                if (result?.Success != true || result.Data == null)
                    return;

                var data = result.Data;

                if (!data.IsActive && data.DaysRemain <= 0)
                {
                    // Trial expired - show blocking overlay
                    ShowBlockingOverlay(data);
                }
                else if (!data.IsActive && data.DaysRemain > 0)
                {
                    // Trial expiring soon - show warning dialog
                    ShowWarningDialog(data);
                }
            }
            catch
            {
                // Ignore trial check failures
            }
        }

        private void ShowBlockingOverlay(IsActiveResponseDto data)
        {
            if (_isActive) return; // Already showing

            _isActive = true;

            // Remove persisted refresh token immediately
            try { TokenPersistence.RemoveRefreshToken(); } catch { }
            try { TokenStore.RefreshToken = null; } catch { }

            string message = data.DaysRemain <= 0
                ? "Your trial period has expired."
                : $"Trial will expire in {data.DaysRemain} day(s).";

            // Create full-screen overlay
            _overlayGrid = new Grid
            {
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(200, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Dialog-like content
            var dialogBorder = new Border
            {
                Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(24),
                MaxWidth = 500,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1)
            };

            var contentStack = new StackPanel { Spacing = 16 };

            contentStack.Children.Add(new TextBlock
            {
                Text = "Trial Mode - Expired",
                FontSize = 20,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            contentStack.Children.Add(new TextBlock
            {
                Text = message + "\nYou must log out or activate to continue.",
                TextWrapping = TextWrapping.Wrap
            });

            var keyBox = new TextBox { PlaceholderText = "Enter activation key", Width = 360 };
            contentStack.Children.Add(keyBox);

            var activationResultText = new TextBlock { Text = string.Empty, Foreground = new SolidColorBrush(Colors.Red) };
            contentStack.Children.Add(activationResultText);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 8
            };

            var activateBtn = new Button
            {
                Content = "Activate",
                Style = (Style)Application.Current.Resources["AccentButtonStyle"]
            };
            var logoutBtn = new Button { Content = "Log Out" };

            buttonStack.Children.Add(activateBtn);
            buttonStack.Children.Add(logoutBtn);
            contentStack.Children.Add(buttonStack);

            dialogBorder.Child = contentStack;
            _overlayGrid.Children.Add(dialogBorder);

            // Block ALL keyboard input
            _overlayGrid.KeyDown += (s, e) => { e.Handled = true; };
            _overlayGrid.KeyUp += (s, e) => { e.Handled = true; };

            // Add to window content
            if (_window.Content is Panel rootPanel)
            {
                rootPanel.Children.Add(_overlayGrid);
            }

            // Wire up buttons
            activateBtn.Click += async (s, e) => await HandleActivateAsync(keyBox, activationResultText, activateBtn);
            logoutBtn.Click += (s, e) => HandleLogout();
        }

        private async Task HandleActivateAsync(TextBox keyBox, TextBlock resultText, Button activateBtn)
        {
            var key = keyBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                resultText.Text = "Please enter an activation key.";
                return;
            }

            resultText.Text = "Verifying...";
            activateBtn.IsEnabled = false;

            try
            {
                var result = await _authService.ActivateAsync(new ActivateRequestDto { Key = key });

                if (result?.Success != true || result.Data == null)
                {
                    resultText.Text = result?.Message ?? "Activation failed.";
                    activateBtn.IsEnabled = true;
                    return;
                }

                if (result.Data.IsValid)
                {
                    // Success: remove overlay
                    RemoveOverlay();

                    var successDialog = new ContentDialog
                    {
                        Title = "Activated",
                        Content = "Activation successful. You may continue.",
                        CloseButtonText = "OK",
                        XamlRoot = _window.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    resultText.Text = "Invalid activation key.";
                    activateBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                resultText.Text = $"Error: {ex.Message}";
                activateBtn.IsEnabled = true;
            }
        }

        private void HandleLogout()
        {
            try { TokenPersistence.RemoveRefreshToken(); } catch { }
            try { TokenStore.RefreshToken = null; } catch { }
            try { TokenStore.AccessToken = null; } catch { }

            AppState.CurrentUser = null;

            var loginWindow = new Views.MainWindow();
            App.MainWindow = loginWindow;
            loginWindow.Activate();

            _window.Close();
        }

        private void RemoveOverlay()
        {
            _isActive = false;
            if (_overlayGrid != null && _window.Content is Panel panel)
            {
                panel.Children.Remove(_overlayGrid);
                _overlayGrid = null;
            }
        }

        private async void ShowWarningDialog(IsActiveResponseDto data)
        {
            string message = $"Trial will expire in {data.DaysRemain} day(s).";

            var stackPanel = new StackPanel { Spacing = 8 };
            stackPanel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });

            var keyBox = new TextBox { PlaceholderText = "Enter activation key", Width = 360 };
            stackPanel.Children.Add(keyBox);

            var activationResultText = new TextBlock { Text = string.Empty, Foreground = new SolidColorBrush(Colors.Red) };
            stackPanel.Children.Add(activationResultText);

            var dialog = new ContentDialog
            {
                Title = "Trial Mode",
                Content = stackPanel,
                PrimaryButtonText = "Log Out",
                CloseButtonText = "Close",
                SecondaryButtonText = "Activate",
                XamlRoot = _window.Content.XamlRoot
            };

            dialog.SecondaryButtonClick += async (s, e) =>
            {
                e.Cancel = true;
                var key = keyBox.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(key))
                {
                    activationResultText.Text = "Please enter an activation key.";
                    return;
                }

                activationResultText.Text = "Verifying...";
                try
                {
                    var result = await _authService.ActivateAsync(new ActivateRequestDto { Key = key });

                    if (result?.Success == true && result.Data?.IsValid == true)
                    {
                        var okDialog = new ContentDialog
                        {
                            Title = "Activated",
                            Content = "Activation successful. You may continue.",
                            CloseButtonText = "OK",
                            XamlRoot = _window.Content.XamlRoot
                        };
                        await okDialog.ShowAsync();
                        dialog.Hide();
                    }
                    else
                    {
                        activationResultText.Text = result?.Message ?? "Invalid activation key.";
                    }
                }
                catch (Exception ex)
                {
                    activationResultText.Text = $"Error: {ex.Message}";
                }
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                HandleLogout();
            }
        }
    }
}
