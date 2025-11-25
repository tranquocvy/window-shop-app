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
using TechHaven.Shared.DTOs.Customers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomerPage : Page
    {
        public CustomerViewModel ViewModel { get; }

        public CustomerPage()
        {
            this.InitializeComponent();

            // 2. Khởi tạo ViewModel
            ViewModel = new CustomerViewModel();

            // 3. Set DataContext của Page (View) chính là ViewModel
            this.DataContext = ViewModel;
        }

        // 4. Khi trang được tải, gọi Command để load dữ liệu
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e); 

            // Force default page size and page number before initial load so request includes PageSize=10
            ViewModel.SelectedPageSize = 10;
            ViewModel.PageNumber = 1;
            ViewModel.LoadCustomersCommand.Execute(null);
        }

        private async void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Dialog để nhận thông tin khách hàng mới
            var nameBox = new TextBox { PlaceholderText = "Tên khách hàng" };
            var phoneBox = new TextBox { PlaceholderText = "Số điện thoại" };
            var emailBox = new TextBox { PlaceholderText = "Email (tuỳ chọn)" };
            var addressBox = new TextBox { PlaceholderText = "Địa chỉ (tuỳ chọn)" };
            var typeCombo = new ComboBox { SelectedIndex = 0 };
            typeCombo.Items.Add(CustomerType.Regular);
            typeCombo.Items.Add(CustomerType.Student);
            typeCombo.Items.Add(CustomerType.VIP);
            var noteBox = new TextBox { PlaceholderText = "Ghi chú (tuỳ chọn)", AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = "Tên" });
            panel.Children.Add(nameBox);
            panel.Children.Add(new TextBlock { Text = "Số điện thoại" });
            panel.Children.Add(phoneBox);
            panel.Children.Add(new TextBlock { Text = "Email" });
            panel.Children.Add(emailBox);
            panel.Children.Add(new TextBlock { Text = "Địa chỉ" });
            panel.Children.Add(addressBox);
            panel.Children.Add(new TextBlock { Text = "Loại khách hàng" });
            panel.Children.Add(typeCombo);
            panel.Children.Add(new TextBlock { Text = "Ghi chú" });
            panel.Children.Add(noteBox);

            var dialog = new ContentDialog
            {
                Title = "Thêm khách hàng",
                PrimaryButtonText = "Lưu",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                Content = panel
            };

            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            // Basic validation
            string name = nameBox.Text?.Trim() ?? string.Empty;
            string phone = phoneBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                var err = new ContentDialog
                {
                    Title = "Thiếu thông tin",
                    Content = "Vui lòng nhập tên và số điện thoại.",
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.Content.XamlRoot;
                await err.ShowAsync();
                return;
            }

            var dto = new CustomerUpsertRequestDto
            {
                CustomerName = name,
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(addressBox.Text) ? null : addressBox.Text.Trim(),
                Type = typeCombo.SelectedItem is CustomerType t ? t : CustomerType.Regular,
                Note = string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim()
            };

            await ViewModel.CreateCustomerAsync(dto);
        }

        private async void CustomersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not CustomerDto selected) return;

            // Coalesce nullable fields to empty string to avoid runtime errors
            var nameBox = new TextBox { Text = selected.CustomerName ?? string.Empty };
            var phoneBox = new TextBox { Text = selected.PhoneNumber ?? string.Empty };
            var emailBox = new TextBox { Text = selected.Email ?? string.Empty };
            var addressBox = new TextBox { Text = selected.Address ?? string.Empty };
            var typeCombo = new ComboBox();
            typeCombo.Items.Add(CustomerType.Regular);
            typeCombo.Items.Add(CustomerType.Student);
            typeCombo.Items.Add(CustomerType.VIP);
            typeCombo.SelectedItem = selected.Type;
            var noteBox = new TextBox { Text = selected.Note ?? string.Empty, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = "Tên" });
            panel.Children.Add(nameBox);
            panel.Children.Add(new TextBlock { Text = "Số điện thoại" });
            panel.Children.Add(phoneBox);
            panel.Children.Add(new TextBlock { Text = "Email" });
            panel.Children.Add(emailBox);
            panel.Children.Add(new TextBlock { Text = "Địa chỉ" });
            panel.Children.Add(addressBox);
            panel.Children.Add(new TextBlock { Text = "Loại khách hàng" });
            panel.Children.Add(typeCombo);
            panel.Children.Add(new TextBlock { Text = "Ghi chú" });
            panel.Children.Add(noteBox);

            var dialog = new ContentDialog
            {
                Title = $"Sửa khách hàng #{selected.CustomerId}",
                PrimaryButtonText = "Lưu",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                Content = panel
            };

            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            string name = nameBox.Text?.Trim() ?? string.Empty;
            string phone = phoneBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                var err = new ContentDialog
                {
                    Title = "Thiếu thông tin",
                    Content = "Vui lòng nhập tên và số điện thoại.",
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.Content.XamlRoot;
                await err.ShowAsync();
                return;
            }

            var dto = new CustomerUpsertRequestDto
            {
                CustomerName = name,
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(addressBox.Text) ? null : addressBox.Text.Trim(),
                Type = typeCombo.SelectedItem is CustomerType t ? t : selected.Type,
                Note = string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim()
            };

            // Skip update if nothing changed
            bool unchanged =
                string.Equals(selected.CustomerName ?? string.Empty, dto.CustomerName ?? string.Empty, StringComparison.Ordinal) &&
                string.Equals(selected.PhoneNumber ?? string.Empty, dto.PhoneNumber ?? string.Empty, StringComparison.Ordinal) &&
                string.Equals(selected.Email ?? string.Empty, dto.Email ?? string.Empty, StringComparison.Ordinal) &&
                string.Equals(selected.Address ?? string.Empty, dto.Address ?? string.Empty, StringComparison.Ordinal) &&
                selected.Type == dto.Type &&
                string.Equals(selected.Note ?? string.Empty, dto.Note ?? string.Empty, StringComparison.Ordinal);

            if (unchanged)
            {
                var info = new ContentDialog
                {
                    Title = "Không có thay đổi",
                    Content = "Thông tin không thay đổi, không cần cập nhật.",
                    CloseButtonText = "Đóng"
                };
                info.XamlRoot = this.Content.XamlRoot;
                await info.ShowAsync();
                return;
            }

            try
            {
                await ViewModel.UpdateCustomerAsync(selected.CustomerId, dto);
            }
            catch (Exception ex)
            {
                var err = new ContentDialog
                {
                    Title = "Lỗi cập nhật",
                    Content = ex.Message,
                    CloseButtonText = "Đóng"
                };
                err.XamlRoot = this.Content.XamlRoot;
                await err.ShowAsync();
            }
        }
    }
}
