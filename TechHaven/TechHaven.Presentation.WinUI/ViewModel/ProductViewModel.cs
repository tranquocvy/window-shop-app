using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ProductViewModel : ObservableObject
    {
        private readonly IProductService _productService = new MockProductService();

        public ObservableCollection<ProductItemViewModel> Products { get; } = new();

        [ObservableProperty]
        private string _searchTerm;

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

        [ObservableProperty]
        private bool _isAllSelected;

        private bool _isUpdatingAll = false;

        partial void OnIsAllSelectedChanged(bool value)
        {
            if (_isUpdatingAll) return; 

            _isUpdatingAll = true;
            foreach (var item in Products)
                item.IsSelected = value;
            _isUpdatingAll = false;
        }


        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var query = new ProductQueryDto
            {
                SearchTerm = this.SearchTerm,
                PageNumber = this.PageNumber,
                PageSize = this.PageSize
            };
            var response = await _productService.QueryProductsAsync(query);
            if (response.Success && response.Data != null)
            {
                foreach (var product in response.Data)
                {
                    var itemVM = new ProductItemViewModel(product);

                    // Subscribe PropertyChanged đúng
                    itemVM.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(ProductItemViewModel.IsSelected))
                        {
                            if (_isUpdatingAll) return; // tránh loop

                            _isUpdatingAll = true;
                            // Chỉ cập nhật IsAllSelected mà không ảnh hưởng item khác
                            IsAllSelected = Products.All(p => p.IsSelected);
                            _isUpdatingAll = false;
                        }
                    };

                    Products.Add(itemVM);
                }

                // Kiểm tra xem tất cả item vừa load có được chọn không
                _isUpdatingAll = true;
                IsAllSelected = Products.All(p => p.IsSelected);
                _isUpdatingAll = false;

                CanGoPrevious = PageNumber > 1;
                CanGoNext = response.Data.Count >= PageSize;
                PageInfo = $"Trang {PageNumber}";
            }
        }


        private void ItemVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductItemViewModel.IsSelected))
            {
                if (_isUpdatingAll) return;

                _isUpdatingAll = true;
                // Cập nhật IsAllSelected dựa trên tất cả item
                IsAllSelected = Products.All(p => p.IsSelected);
                _isUpdatingAll = false;
            }
        }


        [RelayCommand]
        private async Task DeleteSelectedAsync()
        {
            var selectedItems = Products.Where(p => p.IsSelected).ToList();
            foreach (var item in selectedItems)
            {
                var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
                if (response.Success)
                {
                    Products.Remove(item);
                }
            }
        }

        [RelayCommand]
        private async Task DeleteProductContext(ProductItemViewModel item)
        {
            if (item == null) return;

            var response = await _productService.DeleteProductsAsync(item.Product.ProductId);
            if (response.Success)
            {
                Products.Remove(item);
            }
            else
            {
                var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Lỗi",
                    Content = "Xóa sản phẩm thất bại",
                    CloseButtonText = "Đóng"
                };
                _ = dialog.ShowAsync();
            }
        }


        public async Task UpdateProductAsync(int id, ProductCreateUpdateDto dto)
        {
            _ = await _productService.UpdateProductsAsync(id, dto);
            await LoadProductsAsync();
        }


    }


}
