using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Brands;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ProductViewModel : ObservableObject
    {
        // Don't cache HttpClient statically here.
        // Always obtain current client from ApiClientFactory so ResetClient / login changes take effect.
        private HttpClient HttpClient => ApiClientFactory.GetHttpClient();

        // Small concurrency guard to avoid race where multiple LoadProductsAsync calls
        // run concurrently and each applies results (causing duplicated list entries).
        private int _loadInvocationId = 0;

        // Removed static shared HttpClient and the long-lived _productService.
        // We'll create HttpProductService(ApiClientFactory.GetHttpClient()) per operation so auth headers are fresh.

        public ObservableCollection<ProductItemViewModel> Products { get; } = new ObservableCollection<ProductItemViewModel>();

        public ObservableCollection<string> BrandNameFilter { get; } = new ObservableCollection<string>();

        // Search
        [ObservableProperty]
        private string _searchTerm;

        [ObservableProperty]
        private int? _priceFrom;

        [ObservableProperty]
        private int? _priceTo;

        // Pagination
        [ObservableProperty]
        private int _pageNumber = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool _canGoPrevious;

        [ObservableProperty]
        private string _pageInfo;

        // Selection
        [ObservableProperty]
        private bool _isAllSelected;

        private bool _isUpdatingAll = false;

        [ObservableProperty]
        private string _selectedPriceRange;

        public ObservableCollection<string> StatusFilter { get; } = new()
        {
            "Không",
            "Còn hàng",
            "Hết hàng"
        };

        public ObservableCollection<string> PriceRangeOptions { get; } = new()
        {
            "Tất cả",
            "Dưới 5 triệu",
            "Từ 5 đến 15 triệu",
            "Từ 15 đến 30 triệu",
            "Từ 30 đến 50 triệu",
            "Trên 50 triệu"
        };

        [ObservableProperty]
        private string _selectedBrandName = "Không";

        [ObservableProperty]
        private string _selectedStatus = null;

        public ProductViewModel()
        {
            // Load brands when ViewModel is created
            _ = LoadBrandsAsync();
        }

        // ========================
        // Load Brands from API
        // ========================
        public async Task LoadBrandsAsync()
        {
            try
            {
                var client = ApiClientFactory.GetHttpClient();
                var response = await client.GetFromJsonAsync<TechHaven.Shared.DTOs.Common.ResponseWrapper<System.Collections.Generic.List<BrandDto>>>("api/Brand");

                if (response?.Success == true && response.Data != null)
                {
                    BrandNameFilter.Clear();
                    BrandNameFilter.Add("Không"); // Default option

                    foreach (var brand in response.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(brand.BrandName))
                        {
                            BrandNameFilter.Add(brand.BrandName);
                        }
                    }
                }
                else
                {
                    // Fallback if API fails
                    BrandNameFilter.Clear();
                    BrandNameFilter.Add("Không");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load brands: {ex.Message}");
                // Fallback if API fails
                BrandNameFilter.Clear();
                BrandNameFilter.Add("Không");
            }
        }

        // ========================
        // Helper: Build Query
        // ========================
        private ProductListQueryDto BuildQuery()
        {
            ProductStatus? statusFilter = SelectedStatus switch
            {
                "Còn hàng" => ProductStatus.InStock,
                "Hết hàng" => ProductStatus.OutOfStock,
                _ => null
            };

            return new ProductListQueryDto
            {
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim().ToLower(),
                PageNumber = PageNumber,
                PageSize = PageSize,
                Brand = SelectedBrandName == "Không" ? null : SelectedBrandName,

                FromPrice = PriceFrom,
                ToPrice = PriceTo,
                Status = statusFilter
            };
        }

        // ========================
        // Search triggers reload
        // ========================
        partial void OnSearchTermChanged(string value)
        {
            // Reset về trang 1 khi search thay đổi
            PageNumber = 1;
            _ = LoadProductsAsync(BuildQuery());
        }

        // ========================
        // Price range changed -> reload
        // ========================


        partial void OnSelectedBrandNameChanged(string value)
        {
            // Reset về trang 1 khi filter thay đổi
            PageNumber = 1;
            var query = BuildQuery();
            _ = LoadProductsAsync(query);
        }

        partial void OnSelectedStatusChanged(string value)
        {
            // Reset về trang 1 khi filter thay đổi
            PageNumber = 1;
            var query = BuildQuery();
            _ = LoadProductsAsync(query);
        }

        // ========================
        // Page size change
        // ========================
        partial void OnPageSizeChanged(int value)
        {
            PageNumber = 1;
            _ = LoadProductsAsync(BuildQuery());
        }

        // ========================
        // Toggle all selection
        // ========================
        partial void OnIsAllSelectedChanged(bool value)
        {
            if (_isUpdatingAll) return;

            _isUpdatingAll = true;
            foreach (var item in Products)
                item.IsSelected = value;
            _isUpdatingAll = false;
        }

        partial void OnSelectedPriceRangeChanged(string value)
        {
            switch (value)
            {
                case "Dưới 5 triệu":
                    PriceFrom = 0;
                    PriceTo = 5000000;
                    break;

                case "Từ 5 đến 15 triệu":
                    PriceFrom = 5000000;
                    PriceTo = 15000000;
                    break;

                case "Từ 15 đến 30 triệu":
                    PriceFrom = 15000000;
                    PriceTo = 30000000;
                    break;

                case "Từ 30 đến 50 triệu":
                    PriceFrom = 30000000;
                    PriceTo = 50000000;
                    break;

                case "Trên 50 triệu":
                    PriceFrom = 50000000;
                    PriceTo = null;
                    break;

                default: // "Tất cả mức giá"
                    PriceFrom = null;
                    PriceTo = null;
                    break;
            }

            PageNumber = 1;
            _ = LoadProductsAsync(BuildQuery());
        }

        // ========================
        // Main load function
        // ========================
        [RelayCommand]
        public async Task LoadProductsAsync(ProductListQueryDto query = null)
        {
            // Mark invocation id to detect/race and ensure only the latest invocation applies results.
            int invocation = Interlocked.Increment(ref _loadInvocationId);

            // Nếu không truyền query (null), tự động dùng BuildQuery lấy state hiện tại
            query ??= BuildQuery();

            // ==========================
            // Hủy event cũ
            foreach (var item in Products)
                item.PropertyChanged -= ProductItem_PropertyChanged;

            // Xóa toàn bộ list cũ — we clear at start of each invocation.
            Products.Clear();
            // ==========================

            try
            {
                var productService = new HttpProductService(ApiClientFactory.GetHttpClient());
                var response = await productService.QueryProductsAsync(query);

                // If a newer LoadProductsAsync started, drop these results to avoid interleaving/duplication.
                if (invocation != _loadInvocationId)
                {
                    Debug.WriteLine($"[ProductViewModel] LoadProductsAsync invocation {invocation} discarded because newer invocation {_loadInvocationId} exists.");
                    return;
                }

                if (!response.Success || response.Data == null)
                    return;

                var paging = response.Data;

                foreach (var product in paging.Items)
                {
                    var itemVM = new ProductItemViewModel(product);
                    // Đăng ký sự kiện PropertyChanged cho từng item để update Select All checkbox
                    itemVM.PropertyChanged += ProductItem_PropertyChanged;

                    Products.Add(itemVM);
                }

                // Cập nhật trạng thái Select All dựa trên list mới load
                _isUpdatingAll = true;
                IsAllSelected = Products.Any() && Products.All(p => p.IsSelected);
                _isUpdatingAll = false;

                UpdatePaginationState(paging.TotalCount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ProductViewModel] LoadProductsAsync error: {ex}");
            }
        }

        private void ProductItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductItemViewModel.IsSelected))
            {
                if (_isUpdatingAll) return;
                _isUpdatingAll = true;
                IsAllSelected = Products.All(p => p.IsSelected);
                _isUpdatingAll = false;
            }
        }

        // ========================
        // Pagination state update
        // ========================
        private void UpdatePaginationState(int totalCount)
        {
            TotalCount = totalCount;

            TotalPages = PageSize > 0
                ? (int)Math.Ceiling((double)totalCount / PageSize)
                : 1;

            if (TotalPages == 0) TotalPages = 1;

            // Logic kiểm tra trang hiện tại
            if (PageNumber > TotalPages) PageNumber = TotalPages;
            if (PageNumber < 1) PageNumber = 1;

            CanGoPrevious = PageNumber > 1;
            CanGoNext = PageNumber < TotalPages;

            PageInfo = $"Trang {PageNumber}/{TotalPages}";
        }

        // ========================
        // Pagination commands
        // ========================
        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (PageNumber > 1)
            {
                PageNumber--;
                await LoadProductsAsync(BuildQuery());
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (PageNumber < TotalPages)
            {
                PageNumber++;
                await LoadProductsAsync(BuildQuery());
            }
        }

        // ========================
        // Delete selected
        // ========================
        [RelayCommand]
        private async Task DeleteSelectedAsync()
        {
            var selectedItems = Products.Where(p => p.IsSelected).ToList();
            if (!selectedItems.Any())
                return;

            try
            {
                bool confirm = false;
                try
                {
                    confirm = await DialogHelper.ShowConfirmAsync(
                        App.MainWindow,
                        "Xác nhận xóa",
                        $"Bạn có chắc muốn xóa {selectedItems.Count} sản phẩm đã chọn không?"
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeleteSelectedAsync] Dialog error: {ex}");
                    // If dialog cannot show, bail out safely
                    return;
                }

                if (!confirm) return;

                var productService = new HttpProductService(ApiClientFactory.GetHttpClient());

                foreach (var item in selectedItems)
                {
                    try
                    {
                        var response = await productService.DeleteProductsAsync(item.Product.ProductId);
                        if (response?.Success == true)
                            Products.Remove(item);
                        else
                            System.Diagnostics.Debug.WriteLine($"[DeleteSelectedAsync] Failed delete id={item.Product.ProductId} Message={response?.Message}");
                    }
                    catch (HttpRequestException httpEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DeleteSelectedAsync] HTTP error deleting id={item.Product.ProductId}: {httpEx}");
                        // Show a simple error dialog to user
                        var errorDialog = new ContentDialog
                        {
                            Title = "Lỗi mạng",
                            Content = "Không thể kết nối tới máy chủ để xóa sản phẩm.",
                            CloseButtonText = "Đóng",
                            XamlRoot = App.MainWindow?.Content?.XamlRoot
                        };
                        await errorDialog.ShowAsync();
                    }
                }

                // Reload after deletes to keep paging consistent
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteSelectedAsync] Unexpected error: {ex}");
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Đã có lỗi xảy ra khi xóa sản phẩm.",
                    CloseButtonText = "Đóng",
                    XamlRoot = App.MainWindow?.Content?.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        // ========================
        // Context menu delete
        // ========================
        [RelayCommand]
        private async Task DeleteProductContext(ProductItemViewModel item)
        {
            if (item == null) return;

            try
            {
                bool confirm = false;
                try
                {
                    confirm = await DialogHelper.ShowConfirmAsync(
                        App.MainWindow,
                        "Xác nhận xóa",
                        $"Bạn có chắc muốn xóa sản phẩm {item.Product.ProductName} không?"
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeleteProductContext] Dialog error: {ex}");
                    return;
                }

                if (!confirm) return;

                try
                {
                    var productService = new HttpProductService(ApiClientFactory.GetHttpClient());
                    var response = await productService.DeleteProductsAsync(item.Product.ProductId);

                    Products.Remove(item);

                    // Update list/paging after deletion
                    await LoadProductsAsync();

                }
                catch (HttpRequestException httpEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DeleteProductContext] HTTP error deleting id={item.Product.ProductId}: {httpEx}");
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi mạng",
                        Content = "Không thể kết nối tới máy chủ để xóa sản phẩm.",
                        CloseButtonText = "Đóng",
                        XamlRoot = App.MainWindow?.Content?.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DeleteProductContext] Unexpected error: {ex}");
                var errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Đã có lỗi xảy ra khi xóa sản phẩm.",
                    CloseButtonText = "Đóng",
                    XamlRoot = App.MainWindow?.Content?.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        // ========================
        // CRUD operations
        // ========================
        public async Task UpdateProductAsync(int id, ProductUpsertRequest dto)
        {
            var productService = new HttpProductService(ApiClientFactory.GetHttpClient());
            await productService.UpdateProductsAsync(id, dto);
            await LoadProductsAsync(); // Dùng query mặc định
        }

        public async Task CreateProductAsync(ProductUpsertRequest dto)
        {
            if (dto == null) return;
            var productService = new HttpProductService(ApiClientFactory.GetHttpClient());
            var response = await productService.CreateProductsAsync(dto);
            if (response.Success)
                await LoadProductsAsync(); // Dùng query mặc định
        }

        //Kiểm tra phân quyền hiển thị giá nhập
        private string _currentUserRole = AppState.CurrentUser?.RoleName ?? "Seller";

        public string CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                if (SetProperty(ref _currentUserRole, value))
                {
                    OnPropertyChanged(nameof(IsAdmin));
                }
            }
        }

        public bool IsAdmin => CurrentUserRole == "Admin";

        public string CostPriceColumnWidth => IsAdmin ? "1.2*" : "0";
    }



    // ===========================================================
    // Item ViewModel 
    // ===========================================================
    public partial class ProductItemViewModel : ObservableObject
    {
        public ProductDto Product { get; }

        public ProductItemViewModel(ProductDto product)
        {
            Product = product;
        }

        [ObservableProperty]
        private bool _isSelected;

        public string ImageUrl => Product.ImageUrl;

        public string StatusText =>
            Product.StockQuantity > 0 ? "Còn hàng" : "Hết hàng";

        public SolidColorBrush StatusColor =>
            Product.StockQuantity > 0
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);

        // New: provide a faint red background when the product is a draft
        public SolidColorBrush DraftBackground =>
            Product != null && Product.IsDraft
                ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(36, 255, 0, 0)) // alpha ~14%
                : new SolidColorBrush(Colors.Transparent);
    }
}