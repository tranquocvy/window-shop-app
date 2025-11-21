using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views.Controls
{
    public sealed partial class ProductFormUserControl : UserControl
    {
        public ProductFormUserControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Đổ dữ liệu từ ProductDto vào form (Edit mode)
        /// </summary>
        public void LoadData(ProductDto product)
        {
            if (product == null) return;

            // Reset lỗi cũ nếu có
            ClearErrors();

            ProductNameBox.Text = product.ProductName ?? string.Empty;
            BrandNameBox.Text = product.BrandName ?? string.Empty;
            DescriptionBox.Text = product.Description ?? string.Empty;

            SellPriceBox.Value = (double)product.SellPrice;
            StockQuantityBox.Value = product.StockQuantity;

            ColorBox.Text = product.Color ?? string.Empty;

            StorageCapacityBox.Value = product.StorageCapacity ?? double.NaN;
            ProcessorBox.Text = product.Processor ?? string.Empty;
            ScreenSizeBox.Value = (double?)product.ScreenSize ?? double.NaN;
            BatteryCapacityBox.Value = product.BatteryCapacity ?? double.NaN;

            ImageUrlBox.Text = product.ImageUrl ?? string.Empty;
            ImageGalleryJsonBox.Text = product.ImageGalleryJson ?? string.Empty;
        }

        /// <summary>
        /// Lấy dữ liệu từ form + validate hiển thị lỗi UI
        /// </summary>
        public ProductCreateUpdateDto GetFormData()
        {
            // 1. Reset trạng thái lỗi (Ẩn hết thông báo đỏ)
            ClearErrors();

            bool isValid = true;

            // 2. Lấy dữ liệu thô an toàn
            string rawName = ProductNameBox.Text?.Trim();
            string brandName = BrandNameBox.Text?.Trim();
            double sellPrice = GetDoubleSafe(SellPriceBox.Value);
            double stockQty = GetDoubleSafe(StockQuantityBox.Value);

            // 3. Validate từng trường

            // -- Check Tên --
            if (string.IsNullOrEmpty(rawName))
            {
                ProductNameErrorText.Visibility = Visibility.Visible; 
                isValid = false;
            }

            // -- Check Tên Thương hiệu --
            if (string.IsNullOrEmpty(brandName))
            {
                BrandNameErrorText.Visibility = Visibility.Visible; 
                isValid = false;
            }

            // -- Check Giá --
            if (sellPrice <= 0)
            {
                SellPriceErrorText.Visibility = Visibility.Visible; 
                isValid = false;
            }

            // -- Check Số lượng --
            if (stockQty < 0)
            {
                StockQuantityErrorText.Visibility = Visibility.Visible; 
                isValid = false;
            }

            // Nếu có bất kỳ lỗi nào -> Dừng lại, không hiện Dialog nữa (vì đã có text đỏ)
            if (!isValid)
            {
                return null;
            }

            // 4. Nếu hợp lệ -> Build DTO
            return new ProductCreateUpdateDto
            {
                ProductName = rawName,
                BrandName = GetStringOrNull(BrandNameBox.Text),

                SellPrice = (decimal)sellPrice,
                StockQuantity = (int)stockQty,

                Description = GetStringOrNull(DescriptionBox.Text),
                Color = GetStringOrNull(ColorBox.Text),
                Processor = GetStringOrNull(ProcessorBox.Text),
                ImageUrl = GetStringOrNull(ImageUrlBox.Text),
                ImageGalleryJson = GetStringOrNull(ImageGalleryJsonBox.Text),

                StorageCapacity = IsValidNumber(StorageCapacityBox.Value) ? (int)StorageCapacityBox.Value : null,
                BatteryCapacity = IsValidNumber(BatteryCapacityBox.Value) ? (int)BatteryCapacityBox.Value : null,
                ScreenSize = IsValidNumber(ScreenSizeBox.Value) ? (decimal)ScreenSizeBox.Value : null,

                IsDraft = false
            };
        }


        /// <summary>
        /// Ẩn tất cả thông báo lỗi
        /// </summary>
        private void ClearErrors()
        {
            ProductNameErrorText.Visibility = Visibility.Collapsed;
            SellPriceErrorText.Visibility = Visibility.Collapsed;
            StockQuantityErrorText.Visibility = Visibility.Collapsed;
        }

        private void OnInputChanged(object sender, TextChangedEventArgs e)
        {
            // Khi gõ vào tên sản phẩm, ẩn lỗi ngay lập tức
            if (sender == ProductNameBox)
                ProductNameErrorText.Visibility = Visibility.Collapsed;
        }

        private void OnNumberChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            // Khi sửa số, ẩn lỗi tương ứng
            if (sender == SellPriceBox)
                SellPriceErrorText.Visibility = Visibility.Collapsed;

            if (sender == StockQuantityBox)
                StockQuantityErrorText.Visibility = Visibility.Collapsed;
        }

        // ==================================================
        // HELPERS
        // ==================================================

        private string? GetStringOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private double GetDoubleSafe(double value)
        {
            return double.IsNaN(value) ? 0 : value;
        }

        private bool IsValidNumber(double value)
        {
            return !double.IsNaN(value) && value > 0;
        }

        // hàm này phòng trường hợp cần báo lỗi hệ thống khác (DB error, network...)
        private async System.Threading.Tasks.Task ShowErrorAsync(string title, string content)
        {
            if (this.XamlRoot == null) return;

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Đóng",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}