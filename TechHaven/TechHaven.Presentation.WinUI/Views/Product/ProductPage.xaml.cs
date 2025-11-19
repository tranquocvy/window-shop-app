using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views
{
    // LƯU Ý QUAN TRỌNG: Tên lớp phải là 'partial' và khớp chính xác với x:Class trong XAML
    public sealed partial class ProductPage : Page
    {
        
        public ProductViewModel ViewModel { get; }
        public ProductPage()
        {
            // Hàm tạo này là cần thiết và phải được định nghĩa DUY NHẤT 1 lần.
            this.InitializeComponent();

            ViewModel = new ProductViewModel();

            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel.LoadProductsCommand.Execute(null);
        }

        // -------------------------------------------------------------------
        // PHƯƠNG THỨC XỬ LÝ SỰ KIỆN TỪ XAML
        // -------------------------------------------------------------------

        /// <summary>
        /// Xử lý sự kiện khi nhấn nút "Thêm Sản Phẩm"
        /// </summary>
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Viết logic để mở cửa sổ/dialog thêm sản phẩm mới

            // Ví dụ: Hiện thông báo đơn giản
            // (Bạn cần có một TextBlock hoặc thông báo trong UI để hiển thị)
            // System.Diagnostics.Debug.WriteLine("Nút Thêm Sản Phẩm đã được nhấn!");
        }

        private async Task ShowEditProductDialog(ProductItemViewModel item)
        {
            if (item == null) return;

            var dialog = new ProductEditDialog(item.Product);
            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.UpdateProductAsync(item.Product.ProductId, dialog.EditedProduct);
            }
        }



        /// <summary>
        /// Xử lý sự kiện khi nhấp vào một mục (Item) trong ProductsList
        /// </summary>
        private async void ProductsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ProductItemViewModel;
            await ShowEditProductDialog(item);
        }



        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem &&
                menuItem.DataContext is ProductItemViewModel selected)
            {
                await ShowEditProductDialog(selected);
            }
        }




        // -------------------------------------------------------------------
        // CÁC PHƯƠNG THỨC HỖ TRỢ KHÁC (Tùy chọn)
        // -------------------------------------------------------------------

        // private void LoadStaticData()
        // {
        //     // Logic để tạo dữ liệu tĩnh và gán vào ListView (nếu không dùng ItemTemplate tĩnh)
        // }
    }
}