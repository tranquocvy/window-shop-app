using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ProductViewModel : ObservableObject
    {
        private static readonly HttpClient SharedHttpClient = ApiClientFactory.GetHttpClient();
        private readonly IProductService _productService = new HttpProductService(SharedHttpClient);

        public ObservableCollection<ProductItemViewModel> Products { get; } = new ObservableCollection<ProductItemViewModel>();

        // Search
        [ObservableProperty]
        private string _searchTerm;

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

        public ObservableCollection<string> BrandNameFilter { get; } = new()
        {
            "Không",
            "Iphone",
            "Apple",
            "Nokia"
        };

        [ObservableProperty]
        private string _selectedBrandName = "Không";

        // ========================
        // Helper: Build Query
        // ========================
        private ProductListQueryDto BuildQuery()
        {
            var query = new ProductListQueryDto
            {
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm)
                    ? null
                    : SearchTerm.Trim().ToLower(),

                PageNumber = PageNumber,
                PageSize = PageSize,

                Brand = SelectedBrandName == "Không" ? null : SelectedBrandName
            };


            return query;
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

        partial void OnSelectedBrandNameChanged(string value)
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

        // ========================
        // Main load function
        // ========================
        [RelayCommand]
        private async Task LoadProductsAsync(ProductListQueryDto query = null)
        {
            // Nếu không truyền query (null), tự động dùng BuildQuery lấy state hiện tại
            query ??= BuildQuery();

            Products.Clear();


            var response = await _productService.QueryProductsAsync(query);
            if (!response.Success || response.Data == null)
                return;

            var paging = response.Data;

            foreach (var product in paging.Items)
            {
                var itemVM = new ProductItemViewModel(product);
                // Đăng ký sự kiện PropertyChanged cho từng item để update Select All checkbox
                itemVM.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ProductItemViewModel.IsSelected))
                    {
                        if (_isUpdatingAll) return;
                        _isUpdatingAll = true;
                        IsAllSelected = Products.All(p => p.IsSelected);
                        _isUpdatingAll = false;
                    }
                };

                Products.Add(itemVM);
            }

            // Cập nhật trạng thái Select All dựa trên list mới load
            _isUpdatingAll = true;
            IsAllSelected = Products.Any() && Products.All(p => p.IsSelected);
            _isUpdatingAll = false;

            UpdatePaginationState(paging.TotalCount);
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

            PageInfo = $"Trang {PageNumber}/{TotalPages} (Tổng {TotalCount})";
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

            bool confirm = await DialogHelper.ShowConfirmAsync(
                    App.MainWindow,
                    "Xác nhận xóa",
                    $"Bạn có chắc muốn xóa {selectedItems.Count} sản phẩm đã chọn không?"
                );

            if (!confirm) return;

            foreach (var item in selectedItems)
            {
                var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
                if (response.Success)
                    Products.Remove(item);
            }

            // Tải lại danh sách sau khi xóa (dùng BuildQuery mặc định)
            await LoadProductsAsync();
        }

        // ========================
        // Context menu delete
        // ========================
        [RelayCommand]
        private async Task DeleteProductContext(ProductItemViewModel item)
        {
            if (item == null) return;

            bool confirm = await DialogHelper.ShowConfirmAsync(
                App.MainWindow,
                "Xác nhận xóa",
                $"Bạn có chắc muốn xóa sản phẩm {item.Product.ProductName} không?"
            );

            if (!confirm) return;

            var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
            if (response.Success)
            {
                Products.Remove(item);
                // Cập nhật lại phân trang vì số lượng item thay đổi
                await LoadProductsAsync();
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    XamlRoot = App.MainWindow.Content.XamlRoot, // Fix lỗi XamlRoot nếu cần
                    Title = "Lỗi",
                    Content = "Xóa sản phẩm thất bại",
                    CloseButtonText = "Đóng"
                };
                _ = errorDialog.ShowAsync();
            }
        }

        // ========================
        // CRUD operations
        // ========================
        public async Task UpdateProductAsync(int id, ProductUpsertRequest dto)
        {
            await _productService.UpdateProductsAsync(id, dto);
            await LoadProductsAsync(); // Dùng query mặc định
        }

        public async Task CreateProductAsync(ProductUpsertRequest dto)
        {
            if (dto == null) return;
            var response = await _productService.CreateProductsAsync(dto);
            if (response.Success)
                await LoadProductsAsync(); // Dùng query mặc định
        }

        //Kiểm tra phân quyền hiển thị giá nhập
        private string _currentUserRole = "Staff";

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
    }
}