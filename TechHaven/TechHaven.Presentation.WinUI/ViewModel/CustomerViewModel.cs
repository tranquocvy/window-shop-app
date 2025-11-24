using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Shared.DTOs.Common;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class CustomerViewModel : ObservableObject
    {
        // Service to fetch data - can be injected or use default HttpCustomerService
        private readonly ICustomerService _customerService;

        // CancellationTokenSource for debouncing search
        private CancellationTokenSource? _searchCts;

        // Collection of sortable properties
        public ObservableCollection<string> SortableProperties { get; } = new()
        {
            "Không",
            "ID",
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
        public ObservableCollection<int> PageSizeOptions { get; } = new() {5, 10, 15, 20 };

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
            // Debounced search
            if (e.PropertyName == nameof(SearchTerm))
            {
                _searchCts?.Cancel();
                _searchCts = new CancellationTokenSource();
                var token = _searchCts.Token;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(400, token);
                        if (token.IsCancellationRequested) return;
                        PageNumber = 1;
                        await LoadCustomersAsync();
                    }
                    catch (TaskCanceledException) { }
                }, token);
                return;
            }

            // Page size selection changed
            if (e.PropertyName == nameof(SelectedPageSize))
            {
                PageNumber = 1;
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
            var httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri("https://localhost:7230/") // Replace with your API base URL
            };
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
                // Always send the selected page size (default 10)
                PageSize = SelectedPageSize
            };
        }

        private SortingOption? GetSorting()
        {
            if (string.IsNullOrWhiteSpace(SelectedProperty) || SelectedProperty == "Không")
                return null;

            string sortBy = SelectedProperty switch
            {
                "ID" => "CustomerId",
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
            Customers.Clear();

            // Ensure page size matches selected
            if (PageSize != SelectedPageSize)
            {
                PageSize = SelectedPageSize;
            }

            var query = BuildQuery();

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
