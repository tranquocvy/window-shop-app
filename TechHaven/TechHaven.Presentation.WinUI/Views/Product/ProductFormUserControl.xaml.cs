using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Brands;
using TechHaven.Shared.DTOs.Products;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TechHaven.Presentation.WinUI.Views.Controls
{
    /// <summary>
    /// UserControl for product form (add/edit product)
    /// </summary>
    public sealed partial class ProductFormUserControl : UserControl
    {
        private readonly IProductService _productService;
        private string? _originalImageUrl = null;

        private const int MaxImageCount = 10; // Số lượng ảnh tối đa
        private List<string?> _imageUrls = new List<string?>();
        private int _currentImageIndex = 0;

        /// <summary>
        /// Gets the original image URL (for edit mode)
        /// </summary>
        public string? OriginalImageUrl => _originalImageUrl;

        private Task? _brandsLoadingTask;

        /// <summary>
        /// Default constructor (for XAML designer)
        /// </summary>
        public ProductFormUserControl() : this(new HttpProductService(ApiClientFactory.GetHttpClient())) { }

        /// <summary>
        /// Main constructor for DI
        /// </summary>
        public ProductFormUserControl(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            this.InitializeComponent();

            // Initialize image list with MaxImageCount slots
            for (int i = 0; i < MaxImageCount; i++)
            {
                _imageUrls.Add(null);
            }

            var isAdmin = AppState.CurrentUser?.RoleName == "Admin";
            CostPricePanel.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            // Load brands from API
            _brandsLoadingTask = LoadBrandsAsync();

            // Initialize carousel
            UpdateCarouselUI();
        }

        /// <summary>
        /// Cập nhật hiển thị carousel
        /// </summary>
        private void UpdateCarouselUI()
        {
            // Hiển thị ảnh hiện tại
            string? currentUrl = _imageUrls[_currentImageIndex];
            if (!string.IsNullOrWhiteSpace(currentUrl))
            {
                try
                {
                    CurrentImage.Source = new BitmapImage(new Uri(currentUrl));
                }
                catch
                {
                    CurrentImage.Source = null;
                }
            }
            else
            {
                CurrentImage.Source = null;
            }

            // Cập nhật thumbnails
            UpdateThumbnails();

            

            // Vô hiệu hóa nút nếu cần
            PrevButton.IsEnabled = _currentImageIndex > 0;
            NextButton.IsEnabled = _currentImageIndex < _imageUrls.Count - 1;
        }

        /// <summary>
        /// Cập nhật hiển thị thumbnails
        /// </summary>
        private void UpdateThumbnails()
        {
            ThumbnailsPanel.Children.Clear();

            // Thêm thumbnail cho các ảnh đã có
            for (int i = 0; i < _imageUrls.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(_imageUrls[i]))
                {
                    var border = new Border
                    {
                        Width = 60,
                        Height = 60,
                        BorderThickness = new Thickness(2),
                        BorderBrush = i == _currentImageIndex 
                            ? new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue) 
                            : new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                        CornerRadius = new CornerRadius(4),
                        Background = new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke),
                        Tag = i
                    };

                    var image = new Image
                    {
                        Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill
                    };

                    try
                    {
                        image.Source = new BitmapImage(new Uri(_imageUrls[i]));
                    }
                    catch
                    {
                        image.Source = null;
                    }

                    border.Child = image;
                    border.Tapped += OnThumbnailTapped;
                    ToolTipService.SetToolTip(border, $"Ảnh {i + 1}");

                    ThumbnailsPanel.Children.Add(border);
                }
            }

            // Thêm nút "+" để thêm ảnh mới (luôn hiện sau ảnh cuối cùng nếu chưa đủ MaxImageCount ảnh)
            int totalImages = _imageUrls.Count(url => !string.IsNullOrWhiteSpace(url));
            if (totalImages < MaxImageCount)
            {
                var addBorder = new Border
                {
                    Width = 60,
                    Height = 60,
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke),
                    Tag = -1 // Tag đặc biệt cho nút thêm
                };

                var icon = new FontIcon
                {
                    Glyph = "\uE710", // Add icon
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                    FontSize = 24,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                addBorder.Child = icon;
                addBorder.Tapped += OnAddImageTapped;
                ToolTipService.SetToolTip(addBorder, $"Thêm ảnh mới ({totalImages}/{MaxImageCount})");

                ThumbnailsPanel.Children.Add(addBorder);
            }
        }

        /// <summary>
        /// Click vào thumbnail để chuyển ảnh
        /// </summary>
        private void OnThumbnailTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is int index && index >= 0)
            {
                _currentImageIndex = index;
                UpdateCarouselUI();
            }
        }

        /// <summary>
        /// Click vào nút "+" để thêm ảnh mới
        /// </summary>
        private async void OnAddImageTapped(object sender, TappedRoutedEventArgs e)
        {
            // Tìm vị trí trống đầu tiên để thêm ảnh
            int emptyIndex = -1;
            for (int i = 0; i < _imageUrls.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(_imageUrls[i]))
                {
                    emptyIndex = i;
                    break;
                }
            }

            if (emptyIndex == -1) return; // Không có chỗ trống

            // Chuyển đến vị trí trống và mở file picker
            _currentImageIndex = emptyIndex;
            UpdateCarouselUI();
            
            // Trigger upload
            await UploadImageAtCurrentIndex();
        }

        /// <summary>
        /// Chuyển sang ảnh trước
        /// </summary>
        private void OnPreviousImage(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex > 0)
            {
                _currentImageIndex--;
                UpdateCarouselUI();
            }
        }

        /// <summary>
        /// Chuyển sang ảnh sau
        /// </summary>
        private void OnNextImage(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex < _imageUrls.Count - 1)
            {
                _currentImageIndex++;
                UpdateCarouselUI();
            }
        }

        /// <summary>
        /// Ensure brands are loaded before accessing them
        /// </summary>
        private async Task EnsureBrandsLoadedAsync()
        {
            if (_brandsLoadingTask != null)
            {
                await _brandsLoadingTask;
            }
        }

        /// <summary>
        /// Load danh sách thương hiệu từ API
        /// </summary>
        private async Task LoadBrandsAsync()
        {
            try
            {
                var httpClient = ApiClientFactory.GetHttpClient();
                var response = await httpClient.GetFromJsonAsync<TechHaven.Shared.DTOs.Common.ResponseWrapper<List<BrandDto>>>("api/Brand");
                
                if (response?.Success == true && response.Data != null)
                {
                    var brandNames = response.Data
                        .Where(b => !string.IsNullOrWhiteSpace(b.BrandName))
                        .Select(b => b.BrandName)
                        .ToList();
                    
                    BrandComboBox.ItemsSource = brandNames;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load brands: {ex.Message}");
            }
        }


        /// <summary>
        /// Đổ dữ liệu từ ProductDto vào form (Edit mode)
        /// </summary>
        public async void LoadData(ProductDto product)
        {
            if (product == null) return;

            ClearErrors();

            _originalImageUrl = product.ImageUrl;
            _currentImageIndex = 0;

            // Reset image list with MaxImageCount
            _imageUrls.Clear();
            for (int i = 0; i < MaxImageCount; i++)
            {
                _imageUrls.Add(null);
            }

            // Load main image
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                _imageUrls[0] = product.ImageUrl;
            }

            // Load gallery images
            if (!string.IsNullOrWhiteSpace(product.ImageGalleryJson))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(product.ImageGalleryJson);
                    if (list != null)
                    {
                        // Load gallery images up to (MaxImageCount - 1) to account for main image
                        for (int i = 0; i < list.Count && i < (MaxImageCount - 1); i++)
                        {
                            _imageUrls[i + 1] = list[i];
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Gallery Error] {ex.Message}");
                }
            }

            // Update carousel display
            UpdateCarouselUI();

            // Wait for brands to load first
            await EnsureBrandsLoadedAsync();

            ProductNameBox.Text = product.ProductName ?? string.Empty;
            
            // Set brand selection (now using ItemsSource instead of ComboBoxItem)
            if (!string.IsNullOrEmpty(product.BrandName) && BrandComboBox.ItemsSource != null)
            {
                BrandComboBox.SelectedItem = product.BrandName;
            }
            else
            {
                BrandComboBox.SelectedItem = null;
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
        }

        /// <summary>
        /// Lấy dữ liệu từ form + validate hiển thị lỗi UI
        /// </summary>
        public ProductUpsertRequest GetFormData()
        {
            ClearErrors();

            bool isValid = true;

            string rawName = ProductNameBox.Text?.Trim();
            string? brandName = BrandComboBox.SelectedItem as string;
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

            // Ảnh chính là ảnh đầu tiên trong carousel
            string? mainImageUrl = _imageUrls[0];
            
            // Ảnh gallery là các ảnh còn lại
            var galleryList = new List<string>();
            for (int i = 1; i < _imageUrls.Count; i++)
            {
                if (!string.IsNullOrEmpty(_imageUrls[i]))
                {
                    galleryList.Add(_imageUrls[i]);
                }
            }
            string galleryJsonResult = JsonSerializer.Serialize(galleryList);

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

                ImageUrl = mainImageUrl ?? string.Empty,

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
        /// Chọn/thay đổi ảnh tại vị trí hiện tại trong carousel
        /// </summary>
        private async void OnSelectImageTapped(object sender, TappedRoutedEventArgs e)
        {
            await UploadImageAtCurrentIndex();
        }

        /// <summary>
        /// Upload ảnh tại vị trí hiện tại
        /// </summary>
        private async Task UploadImageAtCurrentIndex()
        {
            // 1. Setup FileOpenPicker
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");

            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            // 2. Chọn file
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    var randomAccess = await file.OpenAsync(FileAccessMode.Read);
                    using (randomAccess)
                    using (var readStream = randomAccess.AsStreamForRead())
                    {
                        // Lấy brandName từ ComboBox
                        string? brandName = BrandComboBox.SelectedItem as string;
                        if (string.IsNullOrWhiteSpace(brandName))
                        {
                            await ShowErrorAsync("Thiếu thương hiệu", "Vui lòng chọn thương hiệu trước khi upload ảnh.");
                            return;
                        }
                        var result = await _productService.UploadImageAsync(
                            readStream,
                            file.Name,
                            file.ContentType,
                            brandName
                        );

                        if (result.Success)
                        {
                            // Lưu URL vào vị trí hiện tại trong carousel
                            _imageUrls[_currentImageIndex] = result.Data;
                            // Cập nhật hiển thị
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[UploadImageAtCurrentIndex] Upload failed: {result.Message}");
                            await ShowErrorAsync("Lỗi upload ảnh", result.Message ?? "Không thể upload ảnh.");
                        }
                        UpdateCarouselUI();
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync("Lỗi ngoại lệ", ex.Message);
                }
            }
        }
    }
}
