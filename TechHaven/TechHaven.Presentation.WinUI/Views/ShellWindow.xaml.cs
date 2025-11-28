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
                ThemeManager.ApplyTo(root);
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
            }
            catch
            {
                // fallback to dashboard
                try { contentFrame.Navigate(typeof(DashboardPage)); } catch { }
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
            Brush? toggleBrush = null;
            try
            {
                if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey("TH.TextPrimary"))
                {
                    toggleBrush = Application.Current.Resources["TH.TextPrimary"] as Brush;
                }
                else
                {
                    // search merged dictionaries recursively
                    var res = FindResourceInMergedDictionaries("TH.TextPrimary");
                    if (res is Brush b) toggleBrush = b;
                }
            }
            catch { }

            // fallback: use system black if resource not found
            if (toggleBrush == null)
            {
                toggleBrush = new SolidColorBrush(Colors.Black);
            }

            // common names used by NavigationView templates
            string[] possibleNames = new[] { "TogglePaneButton", "PaneToggleButton", "TogglePaneToggleButton" };

            Button? toggleButton = null;
            foreach (var name in possibleNames)
            {
                toggleButton = FindDescendant<Button>(navView, name);
                if (toggleButton != null) break;
            }

            // If not found by name, try to find first Button descendant (fallback)
            if (toggleButton == null)
            {
                toggleButton = FindDescendant<Button>(navView);
            }

            if (toggleButton != null)
            {
                // Ensure template is applied
                try { toggleButton.ApplyTemplate(); } catch { }

                // Recursively clear VisualState storyboards to prevent template animations from changing Foreground/Background
                try
                {
                    ClearVisualStateStoryboardsRecursively(toggleButton);
                }
                catch { }

                // Now set the Foreground/Background explicitly and attach callbacks so it stays
                toggleButton.Foreground = toggleBrush;
                toggleButton.Background = new SolidColorBrush(Colors.Transparent);

                var fontIcon = FindDescendant<FontIcon>(toggleButton);
                var symbolIcon = FindDescendant<SymbolIcon>(toggleButton);
                var pathIcon = FindDescendant<PathIcon>(toggleButton);

                if (fontIcon != null) fontIcon.Foreground = toggleBrush;
                if (symbolIcon != null) symbolIcon.Foreground = toggleBrush;
                if (pathIcon != null) pathIcon.Foreground = toggleBrush;

                // Reapply brush if template later modifies it
                bool suppress = false;
                toggleButton.RegisterPropertyChangedCallback(Control.ForegroundProperty, (dep, dp) =>
                {
                    if (suppress) return;
                    try
                    {
                        var ctrl = dep as Control;
                        if (ctrl != null && ctrl.Foreground != toggleBrush)
                        {
                            suppress = true;
                            ctrl.Foreground = toggleBrush;
                            suppress = false;
                        }
                    }
                    catch { }
                });

                void RegisterIconCallback(IconElement icon)
                {
                    bool suppressIcon = false;
                    icon.RegisterPropertyChangedCallback(IconElement.ForegroundProperty, (dep, dp) =>
                    {
                        if (suppressIcon) return;
                        try
                        {
                            var ie = dep as IconElement;
                            if (ie != null && ie.Foreground != toggleBrush)
                            {
                                suppressIcon = true;
                                ie.Foreground = toggleBrush;
                                suppressIcon = false;
                            }
                        }
                        catch { }
                    });
                }

                if (fontIcon != null) RegisterIconCallback(fontIcon);
                if (symbolIcon != null) RegisterIconCallback(symbolIcon);
                if (pathIcon != null) RegisterIconCallback(pathIcon);
            }
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
                        loginWindow.Activate();

                        // 3. Đóng cửa sổ chính này lại
                        this.Close();
                    }
                    // Nếu người dùng nhấn "Hủy", dialog tự đóng và không làm gì cả

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
