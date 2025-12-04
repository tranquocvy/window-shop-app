using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Products;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TechHaven.Presentation.WinUI.Views.Controls
{
    public sealed partial class ProductFormUserControl : UserControl
    {
        private readonly IProductService _productService;
        private string? SelectedImagePath = null;
        private string? _originalImageUrl = null;

        public ProductFormUserControl()
        {
            this.InitializeComponent();

            // Use shared HttpClient from ApiClientFactory and concrete HttpProductService
            _productService = new HttpProductService(ApiClientFactory.GetHttpClient());
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
            BrandComboBox.SelectedItem = null;
            if (!string.IsNullOrEmpty(product.BrandName))
            {
                // Duyệt qua các item trong ComboBox để tìm item trùng tên
                foreach (ComboBoxItem item in BrandComboBox.Items)
                {
                    if (item.Content?.ToString() == product.BrandName)
                    {
                        BrandComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            DescriptionBox.Text = product.Description ?? string.Empty;

            CostPriceBox.Value = product.CostPrice > 0 ? (double)product.CostPrice : double.NaN;
            SellPriceBox.Value = (double)product.SellPrice;
            StockQuantityBox.Value = product.StockQuantity;

            ColorBox.Text = product.Color ?? string.Empty;
            StorageCapacityBox.Value = product.StorageCapacity.HasValue ? (double)product.StorageCapacity.Value : double.NaN;
            ProcessorBox.Text = product.Processor ?? string.Empty;
            ScreenSizeBox.Value = product.ScreenSize.HasValue ? (double)product.ScreenSize.Value : double.NaN;
            BatteryCapacityBox.Value = product.BatteryCapacity.HasValue ? (double)product.BatteryCapacity.Value : double.NaN;

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
            string? brandName = null;
            if (BrandComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                brandName = selectedItem.Content.ToString();
            }
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

            // Logic: Nếu vừa upload ảnh mới (SelectedImagePath có giá trị) thì dùng nó.
            // Nếu không, dùng lại ảnh cũ (_originalImageUrl).
            string? finalImageUrl = !string.IsNullOrEmpty(SelectedImagePath) ? SelectedImagePath : _originalImageUrl;

            var jsonRes = JsonSerializer.Serialize(finalImageUrl, new JsonSerializerOptions { WriteIndented = true });
            System.Diagnostics.Debug.WriteLine($"[Link ANh------------------]:\n{jsonRes}");

            // ========== BUILD DTO ==========
            return new ProductUpsertRequest
            {
                ProductName = rawName,
                BrandName = brandName,

                CostPrice = IsValidNumber(CostPriceBox.Value) ? (decimal)CostPriceBox.Value : 0m,
                SellPrice = (decimal)sellPrice,
                StockQuantity = (int)stockQty,

                Description = GetStringOrNull(DescriptionBox.Text),
                Color = GetStringOrNull(ColorBox.Text),
                Processor = GetStringOrNull(ProcessorBox.Text),
                ImageGalleryJson = GetStringOrNull(ImageGalleryJsonBox.Text),

                StorageCapacity = IsValidNumber(StorageCapacityBox.Value) ? (int)StorageCapacityBox.Value : null,
                BatteryCapacity = IsValidNumber(BatteryCapacityBox.Value) ? (int)BatteryCapacityBox.Value : null,
                ScreenSize = IsValidNumber(ScreenSizeBox.Value) ? (decimal)ScreenSizeBox.Value : null,

                // Bỏ dòng code cứng, dùng biến finalImageUrl đã tính toán ở trên
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
            BrandNameErrorText.Visibility = Visibility.Collapsed;
            SellPriceErrorText.Visibility = Visibility.Collapsed;
            StockQuantityErrorText.Visibility = Visibility.Collapsed;
        }

        private void OnInputChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == ProductNameBox)
                ProductNameErrorText.Visibility = Visibility.Collapsed;
        }

        private void OnBrandSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BrandComboBox.SelectedItem != null)
            {
                BrandNameErrorText.Visibility = Visibility.Collapsed;
            }
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
            // 1. Setup FileOpenPicker
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            // WinUI 3 Desktop boilerplate (Lấy HWND)
            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            // 2. Chọn file
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // --- BƯỚC A: Hiển thị Preview ngay lập tức (UX) ---
                var bitmap = new BitmapImage();
                // Lưu ý: Mở stream WinRT để hiển thị ảnh
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmap.SetSourceAsync(stream);
                }
                ProductImage.Source = bitmap;

                // --- BƯỚC B: Upload lên Server qua Service ---
                try
                {
                    // (Tuỳ chọn) Bật loading indicator tại đây nếu có
                    // LoadingRing.IsActive = true; 
                    // ButtonSelectImage.IsEnabled = false;

                    // Use WinRT IRandomAccessStream then convert to System.IO.Stream
                    var randomAccess = await file.OpenAsync(FileAccessMode.Read);
                    using (randomAccess)
                    using (var readStream = randomAccess.AsStreamForRead())
                    {
                        // Gọi Service đã tách biệt
                        var result = await _productService.UploadImageAsync(
                            readStream,
                            file.Name,
                            file.ContentType // StorageFile tự động nhận diện ContentType (image/png, etc.)
                        );

                        if (result.Success)
                        {
                            // Lấy URL từ Data gán vào biến lưu trữ
                            SelectedImagePath = result.Data;

                            // (Debug) Console.WriteLine($"Upload thành công: {result.Data}");
                        }
                        else
                        {
                            // Upload thất bại -> Thông báo lỗi và reset ảnh
                            await ShowErrorAsync("Lỗi Upload", result.Message);

                            ProductImage.Source = null;
                            SelectedImagePath = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync("Lỗi ngoại lệ", ex.Message);
                    ProductImage.Source = null;
                    SelectedImagePath = null;
                }
                finally
                {
                    // (Tuỳ chọn) Tắt loading
                    // LoadingRing.IsActive = false;
                    // ButtonSelectImage.IsEnabled = true;
                }
            }
        }
    }
}
