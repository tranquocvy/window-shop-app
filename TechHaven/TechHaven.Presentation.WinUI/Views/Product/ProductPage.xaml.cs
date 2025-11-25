using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Views.Controls; 
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ProductPage : Page
    {
        public ProductViewModel ViewModel { get; }

        public ProductPage()
        {
            this.InitializeComponent();
            ViewModel = new ProductViewModel();
            this.DataContext = ViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel.Products.Count == 0)
            {
                ViewModel.LoadProductsCommand.Execute(null);
            }
        }

        // -------------------------------------------------------------------
        // LOGIC HIỂN THỊ DIALOG (DÙNG USERCONTROL TRỰC TIẾP)
        // -------------------------------------------------------------------

        /// <summary>
        /// Hàm chung xử lý việc hiển thị form Thêm/Sửa
        /// </summary>
        /// <param name="itemForEdit">Nếu null là Thêm mới, nếu có giá trị là Sửa</param>
        private async Task ShowProductDialogAsync(ProductItemViewModel itemForEdit = null)
        {
            // 1. Tạo UserControl (Form nhập liệu)
            var productForm = new ProductFormUserControl();

            // Nếu là sửa, nạp dữ liệu cũ vào form
            if (itemForEdit != null)
            {
                productForm.LoadData(itemForEdit.Product);
            }

            // 2. Tạo Dialog "động" ngay tại đây (Thay vì dùng file ProductEditorDialog.xaml rời)
            var dialog = new ContentDialog
            {
                Title = itemForEdit == null ? "Thêm sản phẩm mới" : "Cập nhật sản phẩm",
                PrimaryButtonText = "Lưu",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot, // Bắt buộc trong WinUI 3
                Content = productForm // Nhúng UserControl vào Dialog
            };

            // 3. Xử lý sự kiện khi bấm nút LƯU
            dialog.PrimaryButtonClick += async (s, args) =>
            {
                // Lấy dữ liệu từ form (đã validate bên trong UserControl)
                var resultDto = productForm.GetFormData();

                if (resultDto == null)
                {
                    // Validate thất bại (UserControl đã hiện chữ đỏ) -> Giữ Dialog mở
                    args.Cancel = true;
                }
                else
                {
                    // Dữ liệu OK -> Gọi ViewModel xử lý
                    if (itemForEdit == null)
                    {
                        // Chế độ THÊM
                        await ViewModel.CreateProductAsync(resultDto);
                    }
                    else
                    {
                        // Chế độ SỬA
                        await ViewModel.UpdateProductAsync(itemForEdit.Product.ProductId, resultDto);
                    }
                }
            };

            // 4. Hiển thị Dialog
            await dialog.ShowAsync();
        }

        // -------------------------------------------------------------------
        // PHƯƠNG THỨC XỬ LÝ SỰ KIỆN TỪ XAML (HANDLERS)
        // -------------------------------------------------------------------


        /// <summary>
        /// Xử lý sự kiện khi nhấp vào một mục (Item) trong ProductsList
        /// </summary>
        private async void ProductsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ProductItemViewModel selected)
            {
                await ShowProductDialogAsync(selected);
            }
        }


        /// <summary>
        /// Xử lý sự kiện context menu (Chuột phải -> Xem chi tiết/Sửa)
        /// </summary>
        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem &&
                menuItem.DataContext is ProductItemViewModel selected)
            {
                await ShowProductDialogAsync(selected);
            }
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            await ShowProductDialogAsync();
        }

    }
}