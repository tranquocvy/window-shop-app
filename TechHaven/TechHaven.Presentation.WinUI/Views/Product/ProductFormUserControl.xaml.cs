using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
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

        private List<string?> _galleryUrls = new List<string?> { null, null, null };
        public string? OriginalImageUrl => _originalImageUrl;

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

            _galleryUrls = new List<string?> { null, null, null };
            // Reset UI
            GalleryImg1.Source = null;
            GalleryImg2.Source = null;
            GalleryImg3.Source = null;

            if (!string.IsNullOrWhiteSpace(product.ImageGalleryJson))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(product.ImageGalleryJson);
                    if (list != null)
                    {
                        // Đổ dữ liệu từ JSON vào list nội bộ và UI
                        for (int i = 0; i < list.Count && i < 3; i++)
                        {
                            _galleryUrls[i] = list[i]; // Lưu vào biến nhớ

                            // Hiển thị lên UI
                            if (i == 0) GalleryImg1.Source = new BitmapImage(new Uri(list[i]));
                            if (i == 1) GalleryImg2.Source = new BitmapImage(new Uri(list[i]));
                            if (i == 2) GalleryImg3.Source = new BitmapImage(new Uri(list[i]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Gallery Error] {ex.Message}");
                }
            }
            else
            {
                // Không có gallery → reset ảnh
                GalleryImg1.Source = null;
                GalleryImg2.Source = null;
                GalleryImg3.Source = null;
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

            var cleanGallery = new List<string>();
            foreach (var url in _galleryUrls)
            {
                if (!string.IsNullOrEmpty(url)) cleanGallery.Add(url);
            }
            string galleryJsonResult = JsonSerializer.Serialize(cleanGallery);

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

                ImageGalleryJson = galleryJsonResult,

                StorageCapacity = IsValidNumber(StorageCapacityBox.Value) ? (int)StorageCapacityBox.Value : null,
                BatteryCapacity = IsValidNumber(BatteryCapacityBox.Value) ? (int)BatteryCapacityBox.Value : null,
                ScreenSize = IsValidNumber(ScreenSizeBox.Value) ? (decimal)ScreenSizeBox.Value : null,

                // Bỏ dòng code cứng, dùng biến finalImageUrl đã tính toán ở trên
                ImageUrl = finalImageUrl,

                IsDraft = false
            };
        }

        private async void OnGalleryImageTapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Xác định xem người dùng click vào ảnh số mấy (0, 1 hay 2)
            if (sender is Border border && int.TryParse(border.Tag.ToString(), out int index))
            {
                // 2. Mở File Picker (Code giống hệt phần chọn ảnh chính)
                var picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");

                var window = App.MainWindow;
                var hwnd = WindowNative.GetWindowHandle(window);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    // 3. Upload ảnh
                    try
                    {
                        var randomAccess = await file.OpenAsync(FileAccessMode.Read);
                        using (randomAccess)
                        using (var readStream = randomAccess.AsStreamForRead())
                        {
                            var result = await _productService.UploadImageAsync(
                                readStream,
                                file.Name,
                                file.ContentType
                            );

                            if (result.Success)
                            {
                                // 4. Upload thành công -> Cập nhật List dữ liệu
                                string newUrl = result.Data;
                                _galleryUrls[index] = newUrl;

                                // 5. Cập nhật UI ngay lập tức
                                var bitmap = new BitmapImage(new Uri(newUrl));
                                if (index == 0) GalleryImg1.Source = bitmap;
                                else if (index == 1) GalleryImg2.Source = bitmap;
                                else if (index == 2) GalleryImg3.Source = bitmap;

                                // (Tuỳ chọn) Cập nhật lại TextBox Json để người dùng thấy thay đổi
                                var cleanList = new List<string>();
                                foreach (var u in _galleryUrls) if (!string.IsNullOrEmpty(u)) cleanList.Add(u);
                            }
                            else
                            {
                                await ShowErrorAsync("Lỗi Upload", result.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await ShowErrorAsync("Lỗi", ex.Message);
                    }
                }
            }
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
        private async void OnSelectImageTapped(object sender, TappedRoutedEventArgs e)
        {
            // 1. Setup FileOpenPicker
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            // WinUI 3 Desktop boilerplate (Lấy HWND từ App.MainWindow)
            // Đảm bảo bạn đã fix lỗi App.MainWindow ở các bước trước
            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            // 2. Chọn file
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // A. Hiển thị Preview ngay lập tức (UX)
                var bitmap = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await bitmap.SetSourceAsync(stream);
                }
                ProductImage.Source = bitmap;

                // B. Upload lên Server qua Service
                try
                {
                    var randomAccess = await file.OpenAsync(FileAccessMode.Read);
                    using (randomAccess)
                    using (var readStream = randomAccess.AsStreamForRead())
                    {
                        var result = await _productService.UploadImageAsync(
                            readStream,
                            file.Name,
                            file.ContentType
                        );

                        if (result.Success)
                        {
                            // Lấy URL từ Data gán vào biến lưu trữ
                            SelectedImagePath = result.Data;
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
            }
        }
    }
}
