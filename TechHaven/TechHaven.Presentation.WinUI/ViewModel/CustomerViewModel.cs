using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class CustomerViewModel : ObservableObject
    {
        // 2. Dịch vụ (service) để lấy dữ liệu
        private readonly ICustomerService _customerService = new MockCustomerService();

        // 3. Danh sách khách hàng để binding lên DataGrid
        // Dùng ObservableCollection để UI tự động cập nhật
        public ObservableCollection<CustomerDto> Customers { get; } = new ObservableCollection<CustomerDto>();

        // 4. Property cho ô tìm kiếm
        // [ObservableProperty] sẽ tự tạo ra property tên là "SearchTerm"
        [ObservableProperty]
        private string _searchTerm;

        // PAGING PROPERTIES
        [ObservableProperty]
        private int _pageNumber = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool _canGoPrevious;

        [ObservableProperty]
        private string _pageInfo;

        // 5. Tạo một Command để tải dữ liệu
        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            Customers.Clear();

            var query = new CustomerQueryDto
            {
                SearchTerm = this.SearchTerm,
                PageNumber = this.PageNumber,
                PageSize = this.PageSize
            };

            var response = await _customerService.QueryCustomersAsync(query);

            if (response.Success && response.Data != null)
            {
                foreach (var customer in response.Data)
                {
                    Customers.Add(customer);
                }

                // Update paging state
                CanGoPrevious = PageNumber > 1;
                // If returned items count equals page size, there might be a next page
                CanGoNext = response.Data.Count >= PageSize;
                PageInfo = $"Trang {PageNumber}";
            }
        }

        // 6. Tạo một Command cho nút "Thêm"
        [RelayCommand]
        private void AddCustomer()
        {
            // (Thêm logic mở dialog/trang mới ở đây)
            Console.WriteLine("Nút Add Customer đã được nhấn!");
        }

        // Public API for dialog to create a customer and refresh list
        public async Task CreateCustomerAsync(CustomerCreateUpdateDto dto)
        {
            _ = await _customerService.CreateCustomerAsync(dto);
            // Reload current page to reflect changes
            await LoadCustomersAsync();
        }

        // Public API for dialog to update a customer and refresh list
        public async Task UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
        {
            _ = await _customerService.UpdateCustomerAsync(id, dto);
            await LoadCustomersAsync();
        }

        // NEXT / PREVIOUS PAGE COMMANDS
        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (!CanGoNext) return;
            PageNumber++;
            await LoadCustomersAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (!CanGoPrevious) return;
            PageNumber = Math.Max(1, PageNumber - 1);
            await LoadCustomersAsync();
        }

        // 7. Tự động tìm kiếm khi SearchTerm thay đổi
        // Hàm này sẽ được gọi tự động mỗi khi property "SearchTerm" thay đổi
        partial void OnSearchTermChanged(string value)
        {
            // Reset to first page and load
            PageNumber = 1;
            // fire-and-forget
            _ = LoadCustomersAsync();
        }

        // Called when page size changes
        partial void OnPageSizeChanged(int value)
        {
            PageNumber = 1;
            _ = LoadCustomersAsync();
        }

        // Deprecated helper - kept for compatibility
        private async Task SearchCustomersAsync(string searchTerm)
        {
            PageNumber = 1;
            await LoadCustomersAsync();
        }
    }
}
