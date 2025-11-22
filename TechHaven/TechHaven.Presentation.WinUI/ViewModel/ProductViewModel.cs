using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ProductViewModel : ObservableObject
    {
        private readonly IProductService _productService = new MockProductService();

        public ObservableCollection<ProductItemViewModel> Products { get; } = new();

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



        // ========================
        // Search triggers reload
        // ========================
        partial void OnSearchTermChanged(string value)
        {
            PageNumber = 1;
            _ = LoadProductsAsync();
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
        private async Task LoadProductsAsync()
        {
            Products.Clear();

            var query = new ProductQueryDto
            {
                SearchTerm = SearchTerm,
                PageNumber = PageNumber,
                PageSize = PageSize
            };

            var response = await _productService.QueryProductsAsync(query);
            if (!response.Success || response.Data == null)
                return;

            var paging = response.Data; // PagingResponse<ProductDto>

            // Add items
            foreach (var product in paging.Items)
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

            // Update selection
            _isUpdatingAll = true;
            IsAllSelected = Products.All(p => p.IsSelected);
            _isUpdatingAll = false;

            // --- Update pagination using TotalCount (short way) ---
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

            foreach (var item in selectedItems)
            {
                var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
                if (response.Success)
                    Products.Remove(item);
            }

            await LoadProductsAsync();
        }



        // ========================
        // Context menu delete
        // ========================
        [RelayCommand]
        private async Task DeleteProductContext(ProductItemViewModel item)
        {
            if (item == null) return;

            var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
            if (response.Success)
            {
                Products.Remove(item);
                await LoadProductsAsync();
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Xóa sản phẩm thất bại",
                    CloseButtonText = "Đóng"
                };

                _ = dialog.ShowAsync();
            }
        }



        // ========================
        // CRUD operations
        // ========================
        public async Task UpdateProductAsync(int id, ProductCreateUpdateDto dto)
        {
            await _productService.UpdateProductsAsync(id, dto);
            await LoadProductsAsync();
        }

        public async Task CreateProductAsync(ProductCreateUpdateDto dto)
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

        public string StatusText =>
            Product.StockQuantity > 0 ? "Còn hàng" : "Hết hàng";

        public SolidColorBrush StatusColor =>
            Product.StockQuantity > 0
                ? new SolidColorBrush(Colors.Green)
                : new SolidColorBrush(Colors.Red);
    }
}
