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
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class CustomerViewModel : ObservableObject
    {
        // Service to fetch data - can be injected or use default HttpCustomerService
        private readonly ICustomerService _customerService;

        // Collection of customers for data binding
        public ObservableCollection<CustomerDto> Customers { get; } = new ObservableCollection<CustomerDto>();

        // Search term property
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

        public CustomerViewModel(ICustomerService customerService = null)
        {
            // Use injected service or create HttpCustomerService with default HttpClient
            _customerService = customerService ?? CreateDefaultHttpCustomerService();
        }

        // Create default HttpCustomerService
        private static ICustomerService CreateDefaultHttpCustomerService()
        {
            var httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri("https://localhost:7139/") // Replace with your API base URL
            };
            return new HttpCustomerService(httpClient);
        }

        // Command to load customers
        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            Customers.Clear();

            var query = new CustomerListQueryDto
            {
                SearchTerm = this.SearchTerm,
                PageNumber = this.PageNumber,
                PageSize = this.PageSize
            };

            var response = await _customerService.QueryCustomersAsync(query);

            if (response.Success && response.Data != null)
            {
                foreach (var customer in response.Data.Items)
                {
                    Customers.Add(customer);
                }

                // Update paging state with accurate information
                var totalPages = response.Data.TotalPages;
                CanGoPrevious = PageNumber > 1;
                // If returned items count equals page size, there might be a next page
                CanGoNext = PageNumber < totalPages;
                PageInfo = $"Trang {PageNumber} / {totalPages} (Tổng: {response.Data.TotalCount} khách hàng)";
            }
        }

        // Command for "Add Customer" button
        [RelayCommand]
        private void AddCustomer()
        {
            // Logic to open dialog/page
            Console.WriteLine("Nút Add Customer đã được nhấn!");
        }

        // Public API for dialog to create a customer and refresh list
        public async Task CreateCustomerAsync(CustomerUpsertRequestDto dto)
        {
            _ = await _customerService.CreateCustomerAsync(dto);
            // Reload current page to reflect changes
            await LoadCustomersAsync();
        }

        // Public API for dialog to update a customer and refresh list
        public async Task UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto)
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

        // Auto search when SearchTerm changes
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
