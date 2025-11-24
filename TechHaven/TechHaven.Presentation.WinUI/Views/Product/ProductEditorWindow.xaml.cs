using Microsoft.UI.Xaml;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ProductEditorWindow : Window
    {
        public ProductDto Result { get; private set; }

        public ProductEditorWindow(ProductDto productToEdit = null)
        {
            this.InitializeComponent();

            if (productToEdit != null)
            {
                ProductForm.LoadData(productToEdit);
            }
        }
    }
}
