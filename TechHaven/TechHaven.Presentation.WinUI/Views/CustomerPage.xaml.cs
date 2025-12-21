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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Shared.DTOs.Common;

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

        // Danh sách các tỉnh thành Việt Nam
        private static readonly List<string> VietnamProvinces = new List<string>
        {
            "Hà Nội",
            "Huế",
            "Hải Phòng",
            "Đà Nẵng",
            "Cần Thơ",
            "TP.HCM",
            "Tuyên Quang",
            "Lào Cai",
            "Thái Nguyên",
            "Phú Thọ",
            "Bắc Ninh",
            "Hưng Yên",
            "Ninh Bình",
            "Quảng Ninh",
            "Cao Bằng",
            "Lạng Sơn",
            "Lai Châu",
            "Điện Biên",
            "Sơn La",
            "Thanh Hóa",
            "Nghệ An",
            "Hà Tĩnh",
            "Quảng Trị",
            "Quảng Ngãi",
            "Gia Lai",
            "Khánh Hòa",
            "Lâm Đồng",
            "Đắk Lắk",
            "Đồng Nai",
            "Tây Ninh",
            "Vĩnh Long",
            "Đồng Tháp",
            "Cà Mau",
            "An Giang"
        };

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

            // Always refresh PageSize from global AppState and reload data
            try
            {
                ViewModel.PageSize = AppState.PageSize;
            }
            catch { }

            // Trigger load using current state
            ViewModel.LoadCustomersCommand.Execute(null);
        }

        private (bool isValid, string errorMessage) ValidateCustomerInput(string name, string phone, string email, string address, string note)
        {
            // Validate CustomerName
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "Vui lòng nhập tên khách hàng.");
            }
            if (name.Length > 150)
            {
                return (false, "Tên khách hàng không được vượt quá 150 ký tự.");
            }
            // NEW: Check if name contains digits
            if (Regex.IsMatch(name, @"\d"))
            {
                return (false, "Tên khách hàng không được chứa số.");
            }

            // Validate PhoneNumber
            if (string.IsNullOrWhiteSpace(phone))
            {
                return (false, "Vui lòng nhập số điện thoại.");
            }
            
            // Remove all non-digit characters for validation
            string digitsOnly = Regex.Replace(phone, @"\D", "");
            
            // NEW: Check if phone has exactly 10 digits and starts with 0
            if (digitsOnly.Length != 10)
            {
                return (false, "Số điện thoại phải có đúng 10 chữ số.");
            }
            if (!digitsOnly.StartsWith("0"))
            {
                return (false, "Số điện thoại phải bắt đầu bằng số 0.");
            }
            
            if (phone.Length > 15)
            {
                return (false, "Số điện thoại không được vượt quá 15 ký tự.");
            }
            if (!Regex.IsMatch(phone, @"^[\d\-\+\(\)\s]+$"))
            {
                return (false, "Số điện thoại chứa ký tự không hợp lệ. Chỉ cho phép số, dấu +, -, (, ), và khoảng trắng.");
            }

            // Validate Email (optional)
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (email.Length > 150)
                {
                    return (false, "Email không được vượt quá 150 ký tự.");
                }
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return (false, "Định dạng email không hợp lệ.");
                }
            }

            // Validate Address (optional)
            if (!string.IsNullOrWhiteSpace(address) && address.Length > 300)
            {
                return (false, "Địa chỉ không được vượt quá 300 ký tự.");
            }

            // Validate Note (optional)
            if (!string.IsNullOrWhiteSpace(note) && note.Length > 255)
            {
                return (false, "Ghi chú không được vượt quá 255 ký tự.");
            }

            return (true, string.Empty);
        }

        private async void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            bool dialogActive = true;
            string lastName = string.Empty;
            string lastPhone = string.Empty;
            string lastEmail = string.Empty;
            string lastProvince = string.Empty;
            CustomerType lastType = CustomerType.Regular;
            string lastNote = string.Empty;
            
            while (dialogActive)
            {
                // Dialog để nhận thông tin khách hàng mới
                var nameBox = new TextBox { PlaceholderText = "Tên khách hàng", Text = lastName };
                var phoneBox = new TextBox { PlaceholderText = "Số điện thoại", Text = lastPhone };
                var emailBox = new TextBox { PlaceholderText = "Email (tuỳ chọn)", Text = lastEmail };
                
                // Province ComboBox
                var provinceCombo = new ComboBox 
                { 
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    PlaceholderText = "Chọn tỉnh/thành phố"
                };
                foreach (var provinceName in VietnamProvinces)
                {
                    provinceCombo.Items.Add(provinceName);
                }
                if (!string.IsNullOrEmpty(lastProvince))
                {
                    provinceCombo.SelectedItem = lastProvince;
                }
                
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
                panel.Children.Add(new TextBlock { Text = "Tỉnh/Thành phố" });
                panel.Children.Add(provinceCombo);
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

                // Get and trim all input values
                string name = nameBox.Text?.Trim() ?? string.Empty;
                string phone = phoneBox.Text?.Trim() ?? string.Empty;
                string email = emailBox.Text?.Trim() ?? string.Empty;
                string province = provinceCombo.SelectedItem as string ?? string.Empty;
                string note = noteBox.Text?.Trim() ?? string.Empty;
                
                // Save current input for reopening dialog
                lastName = name;
                lastPhone = phone;
                lastEmail = email;
                lastProvince = province;
                lastType = typeCombo.SelectedItem is CustomerType t ? t : CustomerType.Regular;
                lastNote = note;
                
                // Comprehensive validation
                var (isValid, errorMessage) = ValidateCustomerInput(name, phone, email, province, note);
                if (!isValid)
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = errorMessage,
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
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    Address = string.IsNullOrWhiteSpace(province) ? null : province,
                    Type = lastType,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note
                };

                try
                {
                    var resp = await ViewModel.CreateCustomerAsync(dto);
                    if (resp != null && resp.Success)
                    {
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
                    else
                    {
                        var message = resp?.Message ?? "Tạo khách hàng không thành công. Vui lòng thử lại.";
                        var err = new ContentDialog
                        {
                            Title = "Lỗi tạo khách hàng",
                            Content = message,
                            CloseButtonText = "Đóng"
                        };
                        err.XamlRoot = this.Content.XamlRoot;
                        await err.ShowAsync();
                        continue; // Reopen dialog with user's input
                    }
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
            await EditCustomerAsync(selected);
        }

        private async void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is CustomerDto selected)
            {
                await EditCustomerAsync(selected);
            }
        }

        private async void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.Tag is CustomerDto selected)
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
                    var customerName = selected.CustomerName;
                    
                    try
                    {
                        await ViewModel.DeleteCustomerAsync(selected.CustomerId);
                        
                        var successDialog = new ContentDialog
                        {
                            Title = "Thành công",
                            Content = $"Đã xóa khách hàng '{customerName}' thành công!",
                            CloseButtonText = "Đóng"
                        };
                        successDialog.XamlRoot = this.Content.XamlRoot;
                        await successDialog.ShowAsync();
                    }
                    catch (Exception ex)
                    {
                        var err = new ContentDialog
                        {
                            Title = "Lỗi xóa khách hàng",
                            Content = ex.Message,
                            CloseButtonText = "Đóng"
                        };
                        err.XamlRoot = this.Content.XamlRoot;
                        await err.ShowAsync();
                    }
                }
            }
        }

        private async Task EditCustomerAsync(CustomerDto selected)
        {
            bool dialogActive = true;
            
            while (dialogActive)
            {
                // Coalesce nullable fields to empty string to avoid runtime errors
                var nameBox = new TextBox { Text = selected.CustomerName ?? string.Empty };
                var phoneBox = new TextBox { Text = selected.PhoneNumber ?? string.Empty };
                var emailBox = new TextBox { Text = selected.Email ?? string.Empty };
                
                // Province ComboBox - find matching province from existing address
                var provinceCombo = new ComboBox 
                { 
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    PlaceholderText = "Chọn tỉnh/thành phố"
                };
                foreach (var provinceName in VietnamProvinces)
                {
                    provinceCombo.Items.Add(provinceName);
                }
                
                // Try to match existing address with a province
                string existingProvince = string.Empty;
                if (!string.IsNullOrWhiteSpace(selected.Address))
                {
                    foreach (var provinceName in VietnamProvinces)
                    {
                        if (selected.Address.Equals(provinceName, StringComparison.OrdinalIgnoreCase) ||
                            selected.Address.Contains(provinceName))
                        {
                            existingProvince = provinceName;
                            break;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(existingProvince))
                {
                    provinceCombo.SelectedItem = existingProvince;
                }
                
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
                panel.Children.Add(new TextBlock { Text = "Tỉnh/Thành phố" });
                panel.Children.Add(provinceCombo);
                panel.Children.Add(new TextBlock { Text = "Loại khách hàng" });
                panel.Children.Add(typeCombo);
                panel.Children.Add(new TextBlock { Text = "Ghi chú" });
                panel.Children.Add(noteBox);

                var dialog = new ContentDialog
                {
                    Title = $"Sửa khách hàng {selected.CustomerName}",
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

                // Get and trim all input values
                string name = nameBox.Text?.Trim() ?? string.Empty;
                string phone = phoneBox.Text?.Trim() ?? string.Empty;
                string email = emailBox.Text?.Trim() ?? string.Empty;
                string province = provinceCombo.SelectedItem as string ?? string.Empty;
                string note = noteBox.Text?.Trim() ?? string.Empty;
                CustomerType selectedType = typeCombo.SelectedItem is CustomerType t ? t : selected.Type;
                
                // Comprehensive validation
                var (isValid, errorMessage) = ValidateCustomerInput(name, phone, email, province, note);
                if (!isValid)
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi nhập liệu",
                        Content = errorMessage,
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    
                    // Update selected values to keep user input
                    selected.CustomerName = name;
                    selected.PhoneNumber = phone;
                    selected.Email = email;
                    selected.Address = province;
                    selected.Type = selectedType;
                    selected.Note = note;
                    continue; // Reopen dialog
                }

                var dto = new CustomerUpsertRequestDto
                {
                    CustomerName = name,
                    PhoneNumber = phone,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    Address = string.IsNullOrWhiteSpace(province) ? null : province,
                    Type = selectedType,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note
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
