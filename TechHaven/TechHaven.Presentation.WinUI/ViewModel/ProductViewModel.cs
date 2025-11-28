using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
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
            "Samsung",
            "Nokia"
        };

        [ObservableProperty]
        private string _selectedBrandName = "Không";

        // ========================
        // Search triggers reload
        // ========================
        partial void OnSearchTermChanged(string value)
        {
            PageNumber = 1;

            // Tạo query trực tiếp
            var query = new ProductListQueryDto
            {
                SearchTerm = value,      // lấy từ value mới gõ
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            // Gọi API
            _ = LoadProductsAsync(query);
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
            Products.Clear();

            query ??= new ProductListQueryDto
            {
                SearchTerm = SearchTerm,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var response = await _productService.QueryProductsAsync(query);
            if (!response.Success || response.Data == null)
                return;

            var paging = response.Data;

            foreach (var product in paging.Items)
            {
                if (!Products.Any(p => p.Product.ProductId == product.ProductId))
                {
                    var itemVM = new ProductItemViewModel(product);
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
            }

            _isUpdatingAll = true;
            IsAllSelected = Products.All(p => p.IsSelected);
            _isUpdatingAll = false;

            UpdatePaginationState(paging.TotalCount);
        }






        // ========================
        // Pagination state update
        // ========================
        private void UpdatePaginationState(int totalCount)
        {
            TotalCount = totalCount;

            // tránh chia cho 0
            TotalPages = PageSize > 0
                ? (int)System.Math.Ceiling((double)totalCount / PageSize)
                : 1;

            if (TotalPages == 0) TotalPages = 1;

            // đảm bảo PageNumber hợp lệ (tránh trường hợp xóa item khiến PageNumber > TotalPages)
            if (PageNumber > TotalPages) PageNumber = TotalPages;
            if (PageNumber < 1) PageNumber = 1;

            CanGoPrevious = PageNumber > 1;
            CanGoNext = PageNumber < TotalPages;

            PageInfo = $"Trang {PageNumber}/{TotalPages} (Tổng {TotalCount})";
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


            // Xóa các sản phẩm đã chọn
            foreach (var item in selectedItems)
            {
                var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
                if (response.Success)
                    Products.Remove(item);
            }

            // Tải lại danh sách sau khi xóa
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


            if (!confirm)
                return;

            var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
            if (response.Success)
            {
                Products.Remove(item);
            }
            else
            {
                var errorDialog = new ContentDialog
                {
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
            await LoadProductsAsync();
        }

        public async Task CreateProductAsync(ProductUpsertRequest dto)
        {
            if (dto == null) return;

            var response = await _productService.CreateProductsAsync(dto);
            if (response.Success)
                await LoadProductsAsync();
        }



        // ========================
        // Page size change
        // ========================
        partial void OnPageSizeChanged(int value)
        {
            PageNumber = 1;
            _ = LoadProductsAsync();
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
                await LoadProductsAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (PageNumber < TotalPages)
            {
                PageNumber++;
                await LoadProductsAsync();
            }
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

        // Thêm thuộc tính ImageUrl
        public string ImageUrl => Product.ImageUrl;

        public string StatusText =>
            Product.StockQuantity > 0 ? "Còn hàng" : "Hết hàng";

        public SolidColorBrush StatusColor =>
            Product.StockQuantity > 0
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);
    }

}
