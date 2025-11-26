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
            bool dialogActive = true;
            string lastName = string.Empty;
            string lastPhone = string.Empty;
            string lastEmail = string.Empty;
            string lastAddress = string.Empty;
            CustomerType lastType = CustomerType.Regular;
            string lastNote = string.Empty;
            
            while (dialogActive)
            {
                // Dialog để nhận thông tin khách hàng mới
                var nameBox = new TextBox { PlaceholderText = "Tên khách hàng", Text = lastName };
                var phoneBox = new TextBox { PlaceholderText = "Số điện thoại", Text = lastPhone };
                var emailBox = new TextBox { PlaceholderText = "Email (tuỳ chọn)", Text = lastEmail };
                var addressBox = new TextBox { PlaceholderText = "Địa chỉ (tuỳ chọn)", Text = lastAddress };
                var typeCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
                typeCombo.Items.Add(CustomerType.Regular);
                typeCombo.Items.Add(CustomerType.Student);
                typeCombo.Items.Add(CustomerType.VIP);
                
                // Set selected type
                for (int i = 0; i < typeCombo.Items.Count; i++)
                {
                    if (typeCombo.Items[i] is CustomerType type && type == lastType)
                    {
                        typeCombo.SelectedIndex = i;
                        break;
                    }
                }
                
                var noteBox = new TextBox { PlaceholderText = "Ghi chú (tuỳ chọn)", Text = lastNote, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };

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
                {
                    dialogActive = false;
                    return;
                }

                // Basic validation
                string name = nameBox.Text?.Trim() ?? string.Empty;
                string phone = phoneBox.Text?.Trim() ?? string.Empty;
                
                // Save current input
                lastName = name;
                lastPhone = phone;
                lastEmail = emailBox.Text?.Trim() ?? string.Empty;
                lastAddress = addressBox.Text?.Trim() ?? string.Empty;
                lastType = typeCombo.SelectedItem is CustomerType t ? t : CustomerType.Regular;
                lastNote = noteBox.Text?.Trim() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Vui lòng nhập tên khách hàng.",
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    continue; // Reopen dialog
                }
                
                if (string.IsNullOrWhiteSpace(phone))
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Vui lòng nhập số điện thoại.",
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    continue; // Reopen dialog
                }

                var dto = new CustomerUpsertRequestDto
                {
                    CustomerName = name,
                    PhoneNumber = phone,
                    Email = string.IsNullOrWhiteSpace(lastEmail) ? null : lastEmail,
                    Address = string.IsNullOrWhiteSpace(lastAddress) ? null : lastAddress,
                    Type = lastType,
                    Note = string.IsNullOrWhiteSpace(lastNote) ? null : lastNote
                };

                try
                {
                    await ViewModel.CreateCustomerAsync(dto);
                    
                    // Show success notification
                    var successDialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = $"Đã thêm khách hàng '{name}' thành công!",
                        CloseButtonText = "Đóng"
                    };
                    successDialog.XamlRoot = this.Content.XamlRoot;
                    await successDialog.ShowAsync();
                    
                    dialogActive = false;
                }
                catch (Exception ex)
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi tạo khách hàng",
                        Content = ex.Message,
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    continue; // Reopen dialog with user's input
                }
            }
        }

        private async void CustomersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not CustomerDto selected) return;

            bool dialogActive = true;
            
            while (dialogActive)
            {
                // Coalesce nullable fields to empty string to avoid runtime errors
                var nameBox = new TextBox { Text = selected.CustomerName ?? string.Empty };
                var phoneBox = new TextBox { Text = selected.PhoneNumber ?? string.Empty };
                var emailBox = new TextBox { Text = selected.Email ?? string.Empty };
                var addressBox = new TextBox { Text = selected.Address ?? string.Empty };
                
                var typeCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
                typeCombo.Items.Add(CustomerType.Regular);
                typeCombo.Items.Add(CustomerType.Student);
                typeCombo.Items.Add(CustomerType.VIP);
                
                // Set selected item based on current customer type
                for (int i = 0; i < typeCombo.Items.Count; i++)
                {
                    if (typeCombo.Items[i] is CustomerType type && type == selected.Type)
                    {
                        typeCombo.SelectedIndex = i;
                        break;
                    }
                }
                
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
                    SecondaryButtonText = "Xóa",
                    CloseButtonText = "Hủy",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = panel
                };

                dialog.XamlRoot = this.Content.XamlRoot;

                var result = await dialog.ShowAsync();
                
                // Handle delete
                if (result == ContentDialogResult.Secondary)
                {
                    var confirmDialog = new ContentDialog
                    {
                        Title = "Xác nhận xóa",
                        Content = $"Bạn có chắc chắn muốn xóa khách hàng '{selected.CustomerName}'?",
                        PrimaryButtonText = "Xóa",
                        CloseButtonText = "Hủy",
                        DefaultButton = ContentDialogButton.Close
                    };
                    confirmDialog.XamlRoot = this.Content.XamlRoot;
                    
                    var confirmResult = await confirmDialog.ShowAsync();
                    if (confirmResult == ContentDialogResult.Primary)
                    {
                        // Store customer name before deletion
                        var customerName = selected.CustomerName;
                        
                        // Delete and wait for refresh to complete
                        await ViewModel.DeleteCustomerAsync(selected.CustomerId);
                        
                        // Show success notification AFTER refresh
                        var successDialog = new ContentDialog
                        {
                            Title = "Thành công",
                            Content = $"Đã xóa khách hàng '{customerName}' thành công!",
                            CloseButtonText = "Đóng"
                        };
                        successDialog.XamlRoot = this.Content.XamlRoot;
                        await successDialog.ShowAsync();
                    }
                    dialogActive = false;
                    return;
                }
                
                // User cancelled
                if (result != ContentDialogResult.Primary)
                {
                    dialogActive = false;
                    return;
                }

                // Validate input
                string name = nameBox.Text?.Trim() ?? string.Empty;
                string phone = phoneBox.Text?.Trim() ?? string.Empty;
                
                if (string.IsNullOrWhiteSpace(name))
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Vui lòng nhập tên khách hàng.",
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    // Update selected values to keep user input
                    selected.CustomerName = name;
                    selected.PhoneNumber = phone;
                    selected.Email = emailBox.Text?.Trim();
                    selected.Address = addressBox.Text?.Trim();
                    selected.Type = typeCombo.SelectedItem is CustomerType t ? t : selected.Type;
                    selected.Note = noteBox.Text?.Trim();
                    continue; // Reopen dialog
                }
                
                if (string.IsNullOrWhiteSpace(phone))
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = "Vui lòng nhập số điện thoại.",
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    // Update selected values to keep user input
                    selected.CustomerName = name;
                    selected.PhoneNumber = phone;
                    selected.Email = emailBox.Text?.Trim();
                    selected.Address = addressBox.Text?.Trim();
                    selected.Type = typeCombo.SelectedItem is CustomerType t ? t : selected.Type;
                    selected.Note = noteBox.Text?.Trim();
                    continue; // Reopen dialog
                }

                var dto = new CustomerUpsertRequestDto
                {
                    CustomerName = name,
                    PhoneNumber = phone,
                    Email = string.IsNullOrWhiteSpace(emailBox.Text) ? null : emailBox.Text.Trim(),
                    Address = string.IsNullOrWhiteSpace(addressBox.Text) ? null : addressBox.Text.Trim(),
                    Type = typeCombo.SelectedItem is CustomerType t2 ? t2 : selected.Type,
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
                    dialogActive = false;
                    return;
                }

                try
                {
                    await ViewModel.UpdateCustomerAsync(selected.CustomerId, dto);
                    
                    // Show success notification
                    var successDialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = $"Đã cập nhật khách hàng '{dto.CustomerName}' thành công!",
                        CloseButtonText = "Đóng"
                    };
                    successDialog.XamlRoot = this.Content.XamlRoot;
                    await successDialog.ShowAsync();
                    
                    dialogActive = false;
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
                    
                    // Update selected values to keep user input before retrying
                    selected.CustomerName = name;
                    selected.PhoneNumber = phone;
                    selected.Email = dto.Email;
                    selected.Address = dto.Address;
                    selected.Type = dto.Type;
                    selected.Note = dto.Note;
                    continue; // Reopen dialog with user's input
                }
            }
        }
    }
}
