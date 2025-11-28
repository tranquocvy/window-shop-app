using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Themes;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Controls.Primitives;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            this.InitializeComponent();

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
        }
        private void navView_Loaded(object sender, RoutedEventArgs e)
        {
            navView.SelectedItem = navView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => x.Tag.ToString() == "dashboard");
            contentFrame.Navigate(typeof(DashboardPage)); // <-- Nhớ using .Views

            try
            {
                Brush? toggleBrush = null;
                try
                {
                    if (Application.Current?.Resources != null && Application.Current.Resources.ContainsKey("TH.TextPrimary"))
                    {
                        toggleBrush = Application.Current.Resources["TH.TextPrimary"] as Brush;
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

                    if (fontIcon != null)
                    {
                        bool suppressIcon = false;
                        fontIcon.RegisterPropertyChangedCallback(IconElement.ForegroundProperty, (dep, dp) =>
                        {
                            if (suppressIcon) return;
                            try
                            {
                                var icon = dep as IconElement;
                                if (icon != null && icon.Foreground != toggleBrush)
                                {
                                    suppressIcon = true;
                                    icon.Foreground = toggleBrush;
                                    suppressIcon = false;
                                }
                            }
                            catch { }
                        });
                    }

                    if (symbolIcon != null)
                    {
                        bool suppressIcon = false;
                        symbolIcon.RegisterPropertyChangedCallback(IconElement.ForegroundProperty, (dep, dp) =>
                        {
                            if (suppressIcon) return;
                            try
                            {
                                var icon = dep as IconElement;
                                if (icon != null && icon.Foreground != toggleBrush)
                                {
                                    suppressIcon = true;
                                    icon.Foreground = toggleBrush;
                                    suppressIcon = false;
                                }
                            }
                            catch { }
                        });
                    }

                    if (pathIcon != null)
                    {
                        bool suppressIcon = false;
                        pathIcon.RegisterPropertyChangedCallback(IconElement.ForegroundProperty, (dep, dp) =>
                        {
                            if (suppressIcon) return;
                            try
                            {
                                var icon = dep as IconElement;
                                if (icon != null && icon.Foreground != toggleBrush)
                                {
                                    suppressIcon = true;
                                    icon.Foreground = toggleBrush;
                                    suppressIcon = false;
                                }
                            }
                            catch { }
                        });
                    }
                }
            }
            catch
            {
                // ignore failures during visual tree manipulations
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
    }
}
