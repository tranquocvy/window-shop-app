using CommunityToolkit.Mvvm.ComponentModel;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ProductItemViewModel : ObservableObject
    {
        public ProductDto Product { get; }

        public ProductItemViewModel(ProductDto product)
        {
            Product = product;
        }

        [ObservableProperty]
        private bool _isSelected;
    }
}
