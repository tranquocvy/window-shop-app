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

            ClearErrors();

            ProductNameBox.Text = product.ProductName ?? string.Empty;
            BrandNameBox.Text = product.BrandName ?? string.Empty;
            DescriptionBox.Text = product.Description ?? string.Empty;

            CostPriceBox.Value = product.CostPrice.HasValue
                ? (double)product.CostPrice.Value
                : double.NaN;

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
        public ProductUpsertRequest GetFormData()
        {
            ClearErrors();

            bool isValid = true;

            string rawName = ProductNameBox.Text?.Trim();
            string brandName = BrandNameBox.Text?.Trim();
            double sellPrice = GetDoubleSafe(SellPriceBox.Value);
            double stockQty = GetDoubleSafe(StockQuantityBox.Value);

            // ========== VALIDATION ==========

            if (string.IsNullOrEmpty(rawName))
            {
                ProductNameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(brandName))
            {
                BrandNameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (sellPrice <= 0)
            {
                SellPriceErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (stockQty < 0)
            {
                StockQuantityErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return null;

            // ========== BUILD DTO ==========
            return new ProductUpsertRequest
            {
                ProductName = rawName,
                BrandName = GetStringOrNull(BrandNameBox.Text),

                // --- Thêm CostPrice (nullable) ---
                CostPrice = IsValidNumber(CostPriceBox.Value)
                    ? (decimal)CostPriceBox.Value
                    : null,

                SellPrice = (decimal)sellPrice,
                StockQuantity = (int)stockQty,

                Description = GetStringOrNull(DescriptionBox.Text),
                Color = GetStringOrNull(ColorBox.Text),
                Processor = GetStringOrNull(ProcessorBox.Text),
                ImageUrl = GetStringOrNull(ImageUrlBox.Text),
                ImageGalleryJson = GetStringOrNull(ImageGalleryJsonBox.Text),

                StorageCapacity = IsValidNumber(StorageCapacityBox.Value)
                    ? (int)StorageCapacityBox.Value : null,

                BatteryCapacity = IsValidNumber(BatteryCapacityBox.Value)
                    ? (int)BatteryCapacityBox.Value : null,

                ScreenSize = IsValidNumber(ScreenSizeBox.Value)
                    ? (decimal)ScreenSizeBox.Value : null,

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
            if (sender == ProductNameBox)
                ProductNameErrorText.Visibility = Visibility.Collapsed;
        }


        private void OnNumberChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender == SellPriceBox)
                SellPriceErrorText.Visibility = Visibility.Collapsed;

            if (sender == StockQuantityBox)
                StockQuantityErrorText.Visibility = Visibility.Collapsed;

        }



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
