using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using TechHaven.Shared.DTOs.Products;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace TechHaven.Presentation.WinUI.Views.Controls
{
    public sealed partial class ProductFormUserControl : UserControl
    {
        private string? SelectedImagePath = null;
        private string? _originalImageUrl = null;
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

            SelectedImagePath = null;
            _originalImageUrl = product.ImageUrl;

            ProductNameBox.Text = product.ProductName ?? string.Empty;
            BrandNameBox.Text = product.BrandName ?? string.Empty;
            DescriptionBox.Text = product.Description ?? string.Empty;

            CostPriceBox.Value = product.CostPrice.HasValue ? (double)product.CostPrice.Value : double.NaN;
            SellPriceBox.Value = (double)product.SellPrice;
            StockQuantityBox.Value = product.StockQuantity;

            ColorBox.Text = product.Color ?? string.Empty;
            StorageCapacityBox.Value = product.StorageCapacity ?? double.NaN;
            ProcessorBox.Text = product.Processor ?? string.Empty;
            ScreenSizeBox.Value = (double?)product.ScreenSize ?? double.NaN;
            BatteryCapacityBox.Value = product.BatteryCapacity ?? double.NaN;

            ImageGalleryJsonBox.Text = product.ImageGalleryJson ?? string.Empty;

            // ---- Hiển thị ảnh trực tiếp ----
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                try
                {
                    ProductImage.Source = new BitmapImage(new Uri(product.ImageUrl));
                }
                catch
                {
                    ProductImage.Source = null;
                }
            }
            else
            {
                ProductImage.Source = null;
            }
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

            string? finalImageUrl = !string.IsNullOrEmpty(SelectedImagePath) ? SelectedImagePath : _originalImageUrl;

            // ========== BUILD DTO ==========
            return new ProductUpsertRequest
            {
                ProductName = rawName,
                BrandName = GetStringOrNull(BrandNameBox.Text),

                CostPrice = IsValidNumber(CostPriceBox.Value) ? (decimal)CostPriceBox.Value : null,
                SellPrice = (decimal)sellPrice,
                StockQuantity = (int)stockQty,

                Description = GetStringOrNull(DescriptionBox.Text),
                Color = GetStringOrNull(ColorBox.Text),
                Processor = GetStringOrNull(ProcessorBox.Text),
                ImageGalleryJson = GetStringOrNull(ImageGalleryJsonBox.Text),

                StorageCapacity = IsValidNumber(StorageCapacityBox.Value) ? (int)StorageCapacityBox.Value : null,
                BatteryCapacity = IsValidNumber(BatteryCapacityBox.Value) ? (int)BatteryCapacityBox.Value : null,
                ScreenSize = IsValidNumber(ScreenSizeBox.Value) ? (decimal)ScreenSizeBox.Value : null,

                ImageUrl = finalImageUrl,

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

        /// <summary>
        /// Chọn ảnh từ file picker và hiển thị
        /// </summary>
        private async void OnSelectImageClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            // WinUI 3 Desktop: dùng HWND từ cửa sổ hiện tại
            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var bitmap = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmap.SetSourceAsync(stream);
                }

                ProductImage.Source = bitmap;

                // Lưu đường dẫn ảnh mới để GetFormData dùng
                SelectedImagePath = file.Path;
            }
        }
    }
}
