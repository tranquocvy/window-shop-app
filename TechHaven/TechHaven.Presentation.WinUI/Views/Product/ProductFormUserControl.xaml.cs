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

            ProductNameBox.Text = product.ProductName ?? string.Empty;
            BrandNameBox.Text = product.BrandName ?? string.Empty;
            DescriptionBox.Text = product.Description ?? string.Empty;

            // Chỉ load Giá bán và Số lượng
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
        /// Lấy dữ liệu từ form + validate.
        /// </summary>
        public ProductCreateUpdateDto GetFormData()
        {
            // Lấy giá trị an toàn
            double sellPrice = GetDoubleSafe(SellPriceBox.Value);
            double stockQty = GetDoubleSafe(StockQuantityBox.Value);

            // ===========================
            // 1. Validate bắt buộc
            // ===========================

            if (sellPrice <= 0)
            {
                _ = ShowErrorAsync("Lỗi nhập liệu", "Giá bán phải lớn hơn 0.");
                return null;
            }

            if (stockQty < 0)
            {
                _ = ShowErrorAsync("Lỗi nhập liệu", "Số lượng tồn không được âm.");
                return null;
            }

            // ===========================
            // 2. Build DTO trả về
            // ===========================
            return new ProductCreateUpdateDto
            {
                ProductName = ProductNameBox.Text.Trim(),
                BrandName = GetStringOrNull(BrandNameBox.Text),

                SellPrice = (decimal)sellPrice,
                // Không set CostPrice vì UI không có nhập liệu
                StockQuantity = (int)stockQty,

                Description = GetStringOrNull(DescriptionBox.Text),
                Color = GetStringOrNull(ColorBox.Text),
                Processor = GetStringOrNull(ProcessorBox.Text),
                ImageUrl = GetStringOrNull(ImageUrlBox.Text),
                ImageGalleryJson = GetStringOrNull(ImageGalleryJsonBox.Text),

                StorageCapacity = !double.IsNaN(StorageCapacityBox.Value) && StorageCapacityBox.Value > 0
                    ? (int)StorageCapacityBox.Value : null,

                BatteryCapacity = !double.IsNaN(BatteryCapacityBox.Value) && BatteryCapacityBox.Value > 0
                    ? (int)BatteryCapacityBox.Value : null,

                ScreenSize = !double.IsNaN(ScreenSizeBox.Value) && ScreenSizeBox.Value > 0
                    ? (decimal)ScreenSizeBox.Value : null,

                IsDraft = false
            };
        }

        private string? GetStringOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private double GetDoubleSafe(double value)
        {
            return double.IsNaN(value) ? 0 : value;
        }

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