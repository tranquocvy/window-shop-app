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
using TechHaven.Presentation.WinUI.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
                        CloseButtonText = "Cancel" // Dùng CloseButtonText cho nút "Hủy"s
                    };

                    // 2. Cực kỳ quan trọng: Phải set XamlRoot
                    logoutDialog.XamlRoot = this.Content.XamlRoot; // Hoặc navView.XamlRoot

                    // 3. Hiển thị Dialog và chờ kết quả
                    ContentDialogResult result = await logoutDialog.ShowAsync();

                    // 4. Chỉ đăng xuất nếu người dùng nhấn nút "Đăng xuất"
                    if (result == ContentDialogResult.Primary)
                    {
                        // 1. Xóa trạng thái đăng nhập
                        AppState.CurrentUser = null;

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
    }
}
