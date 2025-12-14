using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using TechHaven.Presentation.WinUI.ViewModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Drawing;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;
using System.Diagnostics;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellWindow : Window
    {
        private AppWindow? _appWindow;
        private readonly SettingViewModel _settingViewModel;
        private bool _isForcedTrialActive = false;

        public ShellWindow()
        {
            this.InitializeComponent();

            _settingViewModel = new SettingViewModel();

            // Extend content into title bar so we can use a custom title area
            try
            {
                var hWnd = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                if (appWindow is not null)
                {
                    _appWindow = appWindow;
                    appWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                    // Set system title bar buttons (minimize/maximize/close) colors
                    try
                    {
                        var transparent = Colors.Transparent;
                        appWindow.TitleBar.ButtonBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonHoverBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonPressedBackgroundColor = transparent;
                        appWindow.TitleBar.ButtonInactiveBackgroundColor = transparent;

                        // Apply current theme color
                        ApplyTitleBarColors();

                        // Listen for theme changes so titlebar buttons update immediately
                        ThemeManager.ThemeChanged += ThemeManager_ThemeChanged_ForTitlebar;
                        
                        // Unsubscribe when window closes to avoid memory leaks and crashes on re-login
                        this.Closed += (s, e) => 
                        {
                            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged_ForTitlebar;
                        };
                    }
                    catch
                    {
                        // ignore failures applying titlebar colors
                    }
                }
            }
            catch
            {
                // ignore on platforms where Windowing APIs are not available
            }

            if (Content is FrameworkElement root)
            {
                ThemeManager.RegisterRoot(root);

                // Global keyboard accelerator to intercept Escape at root element level
                try
                {
                    var windowEsc = new KeyboardAccelerator { Key = VirtualKey.Escape };
                    windowEsc.Invoked += (s, e) =>
                    {
                        if (_isForcedTrialActive)
                        {
                            e.Handled = true; // swallow ESC when forced dialog is active
                        }
                    };
                    root.KeyboardAccelerators.Add(windowEsc);
                }
                catch { }
            }

            if (AppState.CurrentUser != null)
            {
                // Lấy dữ liệu từ AppState
                string userFullName = AppState.CurrentUser.UserFullName; //
                string roleName = AppState.CurrentUser.RoleName; //

                // Gán dữ liệu lên UI (lên 2 TextBlock bạn vừa tạo)
                currentUserFullNameText.Text = userFullName;
                currentUserRoleText.Text = roleName;

            }

            // Initialize settings and navigate to last visited page (or dashboard)
            _ = InitializeSettingsAndNavigateAsync();
        }

        private async Task InitializeSettingsAndNavigateAsync()
        {
            try
            {
                await _settingViewModel.InitializeAsync();

                // ensure navView is ready
                if (!navView.IsLoaded)
                {
                    // wait for Loaded event
                    var tcs = new TaskCompletionSource<bool>();
                    void handler(object s, RoutedEventArgs e) { navView.Loaded -= handler; tcs.SetResult(true); }
                    navView.Loaded += handler;
                    await tcs.Task;
                }

                var tag = _settingViewModel.LastVisitedPage;
                Type pageType = typeof(DashboardPage);

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    switch (tag)
                    {
                        case "dashboard": pageType = typeof(DashboardPage); break;
                        case "product": pageType = typeof(ProductPage); break;
                        case "order": pageType = typeof(OrderPage); break;
                        case "customer": pageType = typeof(CustomerPage); break;
                        case "report": pageType = typeof(ReportPage); break;
                        case "setting": pageType = typeof(SettingPage); break;
                        default: pageType = typeof(DashboardPage); break;
                    }

                    // try to select the nav item if available
                    var navItem = navView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => (x.Tag?.ToString()) == tag);
                    if (navItem != null) navView.SelectedItem = navItem;
                }

                // Navigate frame
                contentFrame.Navigate(pageType);

                // After navigation, check trial status
                _ = CheckTrialAsync();
            }
            catch
            {
                // fallback to dashboard
                try { contentFrame.Navigate(typeof(DashboardPage)); } catch { }
            }
        }

        public Task TriggerTrialCheckAsync()
        {
            return CheckTrialAsync();
        }

        private async Task CheckTrialAsync()
        {
            try
            {
                if (AppState.CurrentUser == null) return;
                if (string.IsNullOrWhiteSpace(TokenStore.AccessToken)) return;

                var client = ApiClientFactory.GetHttpClient();

                // prepare request without mutating shared DefaultRequestHeaders (set per-request)
                using var req = new HttpRequestMessage(HttpMethod.Get, "api/Auth/isActive");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenStore.AccessToken);

                HttpResponseMessage resp;
                try
                {
                    resp = await client.SendAsync(req).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                 if (!resp.IsSuccessStatusCode) return;

                 var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<IsActiveResponseDto>>().ConfigureAwait(false);
                 if (wrapper == null || !wrapper.Success || wrapper.Data == null) return;

                 var dto = wrapper.Data;
                 Debug.WriteLine(dto.IsActive ? "True" : "False");

                if (!dto.IsActive)
                {
                    _ = this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        string message = "Your trial period has expired.";
                        if (dto.DaysRemain > 0)
                        {
                            message = $"Trial will expire in {dto.DaysRemain} day(s).";
                        }

                        // If DaysRemain == 0: force user to logout (but remove persisted refresh token immediately).
                        if (dto.DaysRemain <= 0)
                        {
                            try { TokenPersistence.RemoveRefreshToken(); } catch { }
                            try { TokenStore.RefreshToken = null; } catch { }

                            // Create custom blocking overlay instead of ContentDialog to prevent ALL escape routes
                            _isForcedTrialActive = true;
                            
                            // Create full-screen overlay Grid
                            var overlayGrid = new Grid
                            {
                                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(200, 0, 0, 0)),
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch
                            };

                            // Create dialog-like content in center
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
                            
                            // Title
                            contentStack.Children.Add(new TextBlock 
                            { 
                                Text = "Trial Mode - Expired", 
                                FontSize = 20, 
                                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold 
                            });

                            // Message
                            contentStack.Children.Add(new TextBlock 
                            { 
                                Text = message + "\nYou must log out or activate to continue.", 
                                TextWrapping = TextWrapping.Wrap 
                            });

                            // Activation input
                            var keyBox = new TextBox { PlaceholderText = "Enter activation key", Width = 360 };
                            contentStack.Children.Add(keyBox);

                            var activationResultText = new TextBlock { Text = string.Empty, Foreground = new SolidColorBrush(Colors.Red) };
                            contentStack.Children.Add(activationResultText);

                            // Buttons
                            var buttonStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
                            
                            var activateBtn = new Button { Content = "Activate", Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
                            var logoutBtn = new Button { Content = "Log Out" };
                            
                            buttonStack.Children.Add(activateBtn);
                            buttonStack.Children.Add(logoutBtn);
                            contentStack.Children.Add(buttonStack);

                            dialogBorder.Child = contentStack;
                            overlayGrid.Children.Add(dialogBorder);

                            // Block ALL keyboard input on overlay
                            overlayGrid.KeyDown += (s, e) => { e.Handled = true; };
                            overlayGrid.KeyUp += (s, e) => { e.Handled = true; };

                            // Add overlay to root Grid (assuming root is Grid in XAML)
                            if (this.Content is Panel rootPanel)
                            {
                                rootPanel.Children.Add(overlayGrid);
                            }

                            // Handle Activate button
                            activateBtn.Click += async (s, e) =>
                            {
                                var key = keyBox.Text?.Trim() ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(key))
                                {
                                    activationResultText.Text = "Please enter an activation key.";
                                    return;
                                }

                                activationResultText.Text = "Verifying...";
                                activateBtn.IsEnabled = false;

                                try
                                {
                                    var client = ApiClientFactory.GetHttpClient();
                                    var dtoReq = new Shared.DTOs.Auth.ActivateRequestDto { Key = key };
                                    using var resp = await client.PostAsJsonAsync("api/Auth/activate", dtoReq);
                                    
                                    if (!resp.IsSuccessStatusCode)
                                    {
                                        activationResultText.Text = $"Activation failed: {resp.StatusCode}";
                                        activateBtn.IsEnabled = true;
                                        return;
                                    }

                                    var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<ActivateResponseDto>>();
                                    if (wrapper == null || !wrapper.Success || wrapper.Data == null)
                                    {
                                        activationResultText.Text = wrapper?.Message ?? "Activation failed.";
                                        activateBtn.IsEnabled = true;
                                        return;
                                    }

                                    if (wrapper.Data.IsValid)
                                    {
                                        // Success: remove overlay and restore
                                        _isForcedTrialActive = false;
                                        if (this.Content is Panel panel)
                                        {
                                            panel.Children.Remove(overlayGrid);
                                        }

                                        var successDialog = new ContentDialog 
                                        { 
                                            Title = "Activated", 
                                            Content = "Activation successful. You may continue.", 
                                            CloseButtonText = "OK", 
                                            XamlRoot = this.Content.XamlRoot 
                                        };
                                        await successDialog.ShowAsync();
                                    }
                                    else
                                    {
                                        activationResultText.Text = "Invalid activation key.";
                                        activateBtn.IsEnabled = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    activationResultText.Text = $"Error: {ex.Message}";
                                    activateBtn.IsEnabled = true;
                                }
                            };

                            // Handle Log Out button
                            logoutBtn.Click += (s, e) =>
                            {
                                // Perform logout
                                try { TokenPersistence.RemoveRefreshToken(); } catch { }
                                try { TokenStore.RefreshToken = null; } catch { }
                                try { TokenStore.AccessToken = null; } catch { }

                                AppState.CurrentUser = null;

                                var loginWindow = new MainWindow();
                                App.MainWindow = loginWindow;
                                loginWindow.Activate();

                                this.Close();
                            };
                        }
                        else
                        {
                            // DaysRemain > 0: allow user to close dialog and continue, but also provide activation input
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
                                XamlRoot = this.Content.XamlRoot
                            };

                            dialog.SecondaryButtonClick += async (s, e) =>
                            {
                                e.Cancel = true; // keep dialog open while processing
                                var key = keyBox.Text?.Trim() ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(key))
                                {
                                    activationResultText.Text = "Please enter an activation key.";
                                    return;
                                }

                                activationResultText.Text = string.Empty;
                                try
                                {
                                    var client = ApiClientFactory.GetHttpClient();
                                    var dtoReq = new Shared.DTOs.Auth.ActivateRequestDto { Key = key };
                                    using var resp = await client.PostAsJsonAsync("api/Auth/activate", dtoReq).ConfigureAwait(false);
                                    if (!resp.IsSuccessStatusCode)
                                    {
                                        _ = this.DispatcherQueue.TryEnqueue(() => activationResultText.Text = $"Activation failed: {resp.StatusCode}");
                                        return;
                                    }

                                    var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<ActivateResponseDto>>().ConfigureAwait(false);
                                    if (wrapper == null || !wrapper.Success || wrapper.Data == null)
                                    {
                                        _ = this.DispatcherQueue.TryEnqueue(() => activationResultText.Text = wrapper?.Message ?? "Activation failed.");
                                        return;
                                    }

                                    if (wrapper.Data.IsValid)
                                    {
                                        _ = this.DispatcherQueue.TryEnqueue(async () =>
                                        {
                                            var okDialog = new ContentDialog { Title = "Activated", Content = "Activation successful. You may continue.", CloseButtonText = "OK", XamlRoot = this.Content.XamlRoot };
                                            await okDialog.ShowAsync();
                                            dialog.Hide();
                                        });
                                    }
                                    else
                                    {
                                        _ = this.DispatcherQueue.TryEnqueue(() => activationResultText.Text = "Invalid activation key.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _ = this.DispatcherQueue.TryEnqueue(() => activationResultText.Text = $"Error: {ex.Message}");
                                }
                            };

                            var result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                try { TokenPersistence.RemoveRefreshToken(); } catch { }
                                try { TokenStore.RefreshToken = null; } catch { }
                                try { TokenStore.AccessToken = null; } catch { }

                                AppState.CurrentUser = null;

                                var loginWindow = new MainWindow();
                                App.MainWindow = loginWindow;
                                loginWindow.Activate();

                                this.Close();
                            }
                        }
                     });
                 }
             }
             catch
             {
                 // ignore trial check failures
             }
         }

        private void ThemeManager_ThemeChanged_ForTitlebar(ThemeManager.ThemeType obj)
        {
            // update titlebar colors on UI thread
            _ = this.DispatcherQueue?.TryEnqueue(() =>
            {
                ApplyTitleBarColors();
                // Also update nav toggle brush if nav already loaded
                try { UpdateNavToggleBrush(); } catch { }
            });
        }

        private void ApplyTitleBarColors()
        {
            try
            {
                if (_appWindow == null) return;

                var foreground = TryGetColorFromResource("TH.TextPrimary", Windows.UI.Color.FromArgb(255, 255, 255, 255));
                _appWindow.TitleBar.ButtonForegroundColor = foreground;
                _appWindow.TitleBar.ButtonHoverForegroundColor = foreground;
                _appWindow.TitleBar.ButtonPressedForegroundColor = foreground;
                _appWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb((byte)Math.Min(255, (int)(foreground.A * 0.7)), foreground.R, foreground.G, foreground.B);
            }
            catch { }
        }

        private void navView_Loaded(object sender, RoutedEventArgs e)
        {
            // only update visual aspects here; actual navigation handled by InitializeSettingsAndNavigateAsync
            try
            {
                UpdateNavToggleBrush();
            }
            catch
            {
                // ignore failures during visual tree manipulations
            }
        }

        private void UpdateNavToggleBrush()
        {
            try
            {
                // Ensure NavigationView itself uses theme resource (set in XAML)
                // Find the toggle button in the nav view template
                string[] possibleNames = new[] { "TogglePaneButton", "PaneToggleButton", "TogglePaneToggleButton" };

                Button? toggleButton = null;
                foreach (var name in possibleNames)
                {
                    toggleButton = FindDescendant<Button>(navView, name);
                    if (toggleButton != null) break;
                }

                if (toggleButton == null)
                {
                    toggleButton = FindDescendant<Button>(navView);
                }

                if (toggleButton != null)
                {
                    try { toggleButton.ApplyTemplate(); } catch { }

                    try
                    {
                        ClearVisualStateStoryboardsRecursively(toggleButton);
                    }
                    catch { }

                    try
                    {
                        toggleButton.ClearValue(Control.ForegroundProperty);
                        toggleButton.ClearValue(Control.BackgroundProperty);

                        var fontIcon = FindDescendant<FontIcon>(toggleButton);
                        var symbolIcon = FindDescendant<SymbolIcon>(toggleButton);
                        var pathIcon = FindDescendant<PathIcon>(toggleButton);

                        if (fontIcon != null) fontIcon.ClearValue(IconElement.ForegroundProperty);
                        if (symbolIcon != null) symbolIcon.ClearValue(IconElement.ForegroundProperty);
                        if (pathIcon != null) pathIcon.ClearValue(IconElement.ForegroundProperty);
                    }
                    catch { }
                }
            }
            catch { }
        }

        // Recursively clears Storyboard on VisualStates in the subtree
        private static void ClearVisualStateStoryboardsRecursively(DependencyObject node)
        {
            if (node == null) return;

            if (node is FrameworkElement fe)
            {
                try
                {
                    var groups = VisualStateManager.GetVisualStateGroups(fe);
                    if (groups != null)
                    {
                        foreach (var g in groups)
                        {
                            foreach (var s in g.States)
                            {
                                try { s.Storyboard = null; } catch { }
                            }
                        }
                    }
                }
                catch { }
            }

            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(node, i);
                ClearVisualStateStoryboardsRecursively(child);
            }
        }

        // Xử lý khi nhấn vào một item
        private async void navView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer == null) return;

            string? tag = args.InvokedItemContainer.Tag as string;
            if (tag == null) return;

            Type pageType;

            switch (tag)
            {
                case "dashboard":
                    pageType = typeof(DashboardPage);
                    break;
                case "product":
                    pageType = typeof(ProductPage);
                    break;
                case "order":
                    pageType = typeof(OrderPage);
                    break;
                case "customer":
                    pageType = typeof(CustomerPage);
                    break;
                case "report":
                    pageType = typeof(ReportPage);
                    break;
                case "setting":
                    pageType = typeof(SettingPage);
                    break;

                // XỬ LÝ LOGOUT QUAN TRỌNG
                case "logout":
                    ContentDialog logoutDialog = new ContentDialog
                    {
                        Title = "Log Out ",
                        Content = "Do you want log out?",
                        PrimaryButtonText = "Log Out",
                        CloseButtonText = "Cancel"
                    };

                    logoutDialog.XamlRoot = this.Content.XamlRoot; // Hoặc navView.XamlRoot

                    // 3. Hiển thị Dialog và chờ kết quả
                    ContentDialogResult result = await logoutDialog.ShowAsync();

                    // 4. Chỉ đăng xuất nếu người dùng nhấn nút "Đăng xuất"
                    if (result == ContentDialogResult.Primary)
                    {
                        // 1. Xóa trạng thái đăng nhập
                        AppState.CurrentUser = null;

                        // Remove persisted refresh token and clear in-memory tokens
                        try
                        {
                            TokenPersistence.RemoveRefreshToken();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Logout: failed to remove persisted token: {ex.Message}");
                        }

                        // clear in-memory tokens
                        try { TokenStore.RefreshToken = null; } catch { }
                        try { TokenStore.AccessToken = null; } catch { }

                        // 2. Mở lại cửa sổ Login (MainWindow)
                        var loginWindow = new MainWindow();
                        App.MainWindow = loginWindow;
                        loginWindow.Activate();

                        // 3. Đóng cửa sổ chính này lại
                        this.Close();
                    }

                    return;

                default:
                    pageType = typeof(DashboardPage);
                    break;
            }

            // Điều hướng Frame đến trang đã chọn
            contentFrame.Navigate(pageType);

            // Persist last visited (ignore logout)
            try
            {
                if (tag != "logout")
                {
                    await _settingViewModel.SetLastVisitedPageAsync(tag);
                }
            }
            catch { }
        }

        // Recursive helper to find descendant in Visual Tree
        private static T? FindDescendant<T>(DependencyObject parent, string? name = null) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    if (string.IsNullOrEmpty(name)) return t;
                    if (child is FrameworkElement fe && fe.Name == name) return t;
                }

                var result = FindDescendant<T>(child, name);
                if (result != null) return result;
            }

            return null;
        }

        private static object? FindResourceInMergedDictionaries(string key)
        {
            try
            {
                if (Application.Current?.Resources != null)
                {
                    // check root resources first
                    if (Application.Current.Resources.ContainsKey(key))
                        return Application.Current.Resources[key];

                    // search merged dictionaries recursively
                    foreach (var md in Application.Current.Resources.MergedDictionaries)
                    {
                        var found = FindInDictionaryRecursive(md, key);
                        if (found != null) return found;
                    }
                }
            }
            catch { }

            return null;
        }

        private static object? FindInDictionaryRecursive(ResourceDictionary dict, string key)
        {
            try
            {
                if (dict == null) return null;
                if (dict.ContainsKey(key)) return dict[key];

                foreach (var md in dict.MergedDictionaries)
                {
                    var f = FindInDictionaryRecursive(md, key);
                    if (f != null) return f;
                }
            }
            catch { }

            return null;
        }

        private static Windows.UI.Color TryGetColorFromResource(string key, Windows.UI.Color fallback)
        {
            try
            {
                // Check the application resources first
                if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey(key))
                {
                    var res = Application.Current.Resources[key];
                    if (res is SolidColorBrush scb)
                        return scb.Color;
                    if (res is Windows.UI.Color c)
                        return c;
                }

                // Search merged dictionaries recursively
                var found = FindResourceInMergedDictionaries(key);
                if (found != null)
                {
                    if (found is SolidColorBrush scb2)
                        return scb2.Color;
                    if (found is Windows.UI.Color c2)
                        return c2;
                }
            }
            catch { }

            return fallback;
        }
    }
}
