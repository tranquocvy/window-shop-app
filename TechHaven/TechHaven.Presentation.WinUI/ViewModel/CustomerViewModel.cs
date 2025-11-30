using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class CustomerViewModel : ObservableObject
    {
        // Service to fetch data - can be injected or use default HttpCustomerService
        private readonly ICustomerService _customerService;

        // CancellationTokenSource for debouncing search (kept for possible future use)
        private CancellationTokenSource? _searchCts;

        // Request counter to identify latest load request and ignore stale responses
        private int _loadRequestCounter = 0;

        // Collection of sortable properties
        public ObservableCollection<string> SortableProperties { get; } = new()
        {
            "Không",
            "Tên",
            "Hạng",
            "Tổng Mua"
        };
        // Collection of sort directions
        public ObservableCollection<string> SortDirections { get; } = new()
        {
            "Không",
            "Tăng dần",
            "Giảm dần"
        };

        // Page size options
        public ObservableCollection<int> PageSizeOptions { get; } = new() { 5, 10, 15, 20 };

        // Collection of customers for data binding
        public ObservableCollection<CustomerDto> Customers { get; } = new ObservableCollection<CustomerDto>();

        // Sortable Properties
        [ObservableProperty]
        private string _selectedProperty = "Không";

        // Sort Direction
        [ObservableProperty]
        private string _selectedDirection = "Không";

        // Search term property
        [ObservableProperty]
        private string _searchTerm;

        // Filter: Customer type - explicit property
        private CustomerType? _selectedType;
        public CustomerType? SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }

        // Filter: CreatedAt date range - explicit properties
        private DateTime? _createdAtStart;
        public DateTime? CreatedAtStart
        {
            get => _createdAtStart;
            set => SetProperty(ref _createdAtStart, value);
        }

        private DateTime? _createdAtEnd;
        public DateTime? CreatedAtEnd
        {
            get => _createdAtEnd;
            set => SetProperty(ref _createdAtEnd, value);
        }

        // Selected page size (bind to UI selection) - explicit property
        private int _selectedPageSize = 10;
        public int SelectedPageSize
        {
            get => _selectedPageSize;
            set => SetProperty(ref _selectedPageSize, value);
        }

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

            // Initialize selected page size
            SelectedPageSize = 10;
            PageSize = SelectedPageSize;

            // Subscribe to property changed to react to filter changes
            this.PropertyChanged += CustomerViewModel_PropertyChanged;

            // NOTE: do not auto-load here; Page.OnNavigatedTo will call LoadCustomersCommand
        }

        private void CustomerViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Immediate search: call API as the user types
            if (e.PropertyName == nameof(SearchTerm))
            {
                // Cancel any previous debounce token (for future use if reintroducing debounce)
                _searchCts?.Cancel();

                // Immediately request first page and load
                PageNumber = 1;
                _ = LoadCustomersAsync();
                return;
            }

            // Page size selection changed
            if (e.PropertyName == nameof(SelectedPageSize))
            {
                PageNumber = 1;
                // Keep PageSize in sync when user explicitly changes page size
                PageSize = SelectedPageSize;
                _ = LoadCustomersAsync();
                return;
            }

            // Other filters -> reset to first page and reload
            if (e.PropertyName == nameof(SelectedProperty)
                || e.PropertyName == nameof(SelectedDirection)
                || e.PropertyName == nameof(SelectedType)
                || e.PropertyName == nameof(CreatedAtStart)
                || e.PropertyName == nameof(CreatedAtEnd))
            {
                PageNumber = 1;
                _ = LoadCustomersAsync();
            }
        }

        // Create default HttpCustomerService
        private static ICustomerService CreateDefaultHttpCustomerService()
        {
            var httpClient = ApiClientFactory.GetHttpClient();
            return new HttpCustomerService(httpClient);
        }

        // Build query from current UI state
        private CustomerListQueryDto BuildQuery()
        {
            return new CustomerListQueryDto
            {
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                Type = SelectedType,
                CreatedAt = (CreatedAtStart.HasValue || CreatedAtEnd.HasValue)
                    ? new DateRangeFilter { StartDate = CreatedAtStart, EndDate = CreatedAtEnd }
                    : null,
                Sorting = GetSorting(),
                PageNumber = PageNumber,
                // Use current PageSize (do not overwrite user's selection when searching)
                PageSize = PageSize > 0 ? PageSize : SelectedPageSize
            };
        }

        private SortingOption? GetSorting()
        {
            if (string.IsNullOrWhiteSpace(SelectedProperty) || SelectedProperty == "Không")
                return null;

            string sortBy = SelectedProperty switch
            {
                "Tên" => "CustomerName",
                "Hạng" => "Type",
                "Tổng Mua" => "TotalPurchased",
                _ => SelectedProperty
            };

            bool desc = SelectedDirection == "Giảm dần";
            if (string.IsNullOrWhiteSpace(sortBy)) return null;
            return new SortingOption { SortBy = sortBy, Desc = desc };
        }

        // Command to load customers
        [RelayCommand]
        private async Task LoadCustomersAsync()
        {
            // Capture request id so we can ignore stale responses
            var requestId = Interlocked.Increment(ref _loadRequestCounter);

            var query = BuildQuery();

            try
            {
                var response = await _customerService.QueryCustomersAsync(query);

                // If a newer request was started, ignore this response
                if (requestId != _loadRequestCounter)
                {
                    Debug.WriteLine("Ignoring stale response for LoadCustomersAsync");
                    return;
                }

                if (response == null)
                {
                    Debug.WriteLine("QueryCustomersAsync returned null response");
                    return;
                }

                if (!response.Success)
                {
                    Debug.WriteLine($"QueryCustomersAsync failed: {response.Message}");
                    if (response.Errors != null)
                    {
                        foreach (var e in response.Errors)
                            Debug.WriteLine(" - " + e);
                    }
                    return;
                }

                if (response.Data?.Items == null)
                {
                    Debug.WriteLine("QueryCustomersAsync returned empty data or items");
                    return;
                }

                // Clear collection before adding latest results (only after response validated)
                Customers.Clear();

                // Prevent duplicates within the incoming page: track IDs seen in this response
                var addedIds = new HashSet<int>();

                foreach (var customer in response.Data.Items)
                {
                    if (customer == null) continue;

                    if (addedIds.Add(customer.CustomerId))
                    {
                        Customers.Add(customer);
                    }
                }

                // Update paging state with accurate information
                var totalPages = response.Data.TotalPages;
                CanGoPrevious = PageNumber > 1;
                CanGoNext = PageNumber < totalPages;
                PageInfo = $"Trang {PageNumber} / {totalPages} (Tổng: {response.Data.TotalCount} khách hàng)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in LoadCustomersAsync: " + ex);
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
            var response = await _customerService.CreateCustomerAsync(dto);
            
            if (response?.Success == true)
            {
                await LoadCustomersAsync();
            }
        }

        // Public API for dialog to update a customer and refresh list
        public async Task UpdateCustomerAsync(int id, CustomerUpsertRequestDto dto)
        {
            var response = await _customerService.UpdateCustomerAsync(id, dto);
            
            if (response?.Success == true)
            {
                await LoadCustomersAsync();
            }
        }

        // Public API for dialog to delete a customer and refresh list
        public async Task DeleteCustomerAsync(int id)
        {
            var response = await _customerService.DeleteCustomerAsync(id);
            
            if (response?.Success == true)
            {
                await LoadCustomersAsync();
            }
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

        // Ensure initial load uses default page size
        public async Task EnsureInitialLoadAsync()
        {
            SelectedPageSize = 10;
            PageNumber = 1;
            PageSize = SelectedPageSize;
            await LoadCustomersAsync();
        }

        // Deprecated helper - kept for compatibility
        private async Task SearchCustomersAsync(string searchTerm)
        {
            PageNumber = 1;
            await LoadCustomersAsync();
        }
    }
}
