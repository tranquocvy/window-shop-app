using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        /// <summary>
        /// Xử lý sự kiện khi nhấp vào một mục (Item) trong ProductsList
        /// </summary>
        private void ProductsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: Viết logic để điều hướng đến trang chi tiết sản phẩm hoặc mở dialog chỉnh sửa

            // Lấy dữ liệu của Item được click
            // var selectedProduct = e.ClickedItem as Product; 

            // if (selectedProduct != null)
            // {
            //     System.Diagnostics.Debug.WriteLine($"Sản phẩm được chọn: {selectedProduct.Name}");
            //     // Ví dụ: Navigation.Frame.Navigate(typeof(ProductDetailPage), selectedProduct);
            // }
        }

        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            // Lấy item gốc từ MenuFlyout
            if (sender is MenuFlyoutItem menuItem &&
                menuItem.DataContext is ProductItemViewModel selected)
            {
                var product = selected.Product;

                // Tạo UI cho dialog
                var nameBox = new TextBox { Text = product.ProductName ?? "" };
                var priceBox = new TextBox { Text = product.SellPrice.ToString() };
                var stockBox = new TextBox { Text = product.StockQuantity.ToString() };
                var brandBox = new TextBox { Text = product.BrandName ?? "" };
                var descBox = new TextBox
                {
                    Text = product.Description ?? "",
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap
                };

                var panel = new StackPanel { Spacing = 8 };
                panel.Children.Add(new TextBlock { Text = "Tên sản phẩm" });
                panel.Children.Add(nameBox);

                panel.Children.Add(new TextBlock { Text = "Giá" });
                panel.Children.Add(priceBox);

                panel.Children.Add(new TextBlock { Text = "Số lượng tồn" });
                panel.Children.Add(stockBox);

                panel.Children.Add(new TextBlock { Text = "Danh mục / Thương hiệu" });
                panel.Children.Add(brandBox);

                panel.Children.Add(new TextBlock { Text = "Mô tả" });
                panel.Children.Add(descBox);

                var dialog = new ContentDialog
                {
                    Title = $"Sửa sản phẩm #{product.ProductId}",
                    PrimaryButtonText = "Lưu",
                    CloseButtonText = "Hủy",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = panel
                };

                dialog.XamlRoot = this.Content.XamlRoot;

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                    return;

                // Validation đơn giản
                if (string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    var err = new ContentDialog
                    {
                        Title = "Thiếu thông tin",
                        Content = "Tên sản phẩm không được để trống.",
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                    return;
                }

                // Tạo DTO update
                var dto = new ProductCreateUpdateDto
                {
                    ProductName = nameBox.Text.Trim(),
                    SellPrice = decimal.TryParse(priceBox.Text, out var price) ? price : product.SellPrice,
                    StockQuantity = int.TryParse(stockBox.Text, out var qty) ? qty : product.StockQuantity,
                    BrandName = string.IsNullOrWhiteSpace(brandBox.Text) ? null : brandBox.Text.Trim(),
                    Description = string.IsNullOrWhiteSpace(descBox.Text) ? null : descBox.Text.Trim()
                };

                // Kiểm tra thay đổi
                bool unchanged =
                    dto.ProductName == product.ProductName &&
                    dto.SellPrice == product.SellPrice &&
                    dto.StockQuantity == product.StockQuantity &&
                    dto.BrandName == product.BrandName &&
                    dto.Description == product.Description;

                if (unchanged)
                {
                    var info = new ContentDialog
                    {
                        Title = "Không có thay đổi",
                        Content = "Bạn chưa thay đổi gì cả.",
                        CloseButtonText = "Đóng"
                    };
                    info.XamlRoot = this.Content.XamlRoot;
                    await info.ShowAsync();
                    return;
                }

                // Gọi ViewModel cập nhật
                try
                {
                    await ViewModel.UpdateProductAsync(product.ProductId, dto);
                }
                catch (Exception ex)
                {
                    var err = new ContentDialog
                    {
                        Title = "Lỗi cập nhật",
                        Content = ex.Message,
                        CloseButtonText = "Đóng"
                    };
                    err.XamlRoot = this.Content.XamlRoot;
                    await err.ShowAsync();
                }
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