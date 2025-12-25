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
    public sealed partial class ProductFormUserControl : UserControl
    {
        private readonly IProductService _productService;
        private string? _originalImageUrl = null;

        private List<string> _imageUrls = new List<string>();
        private int _currentImageIndex = 0;
        
        public string? OriginalImageUrl => _originalImageUrl;

        private Task? _brandsLoadingTask;

        public ProductFormUserControl()
        {
            this.InitializeComponent();

            // Use shared HttpClient from ApiClientFactory and concrete HttpProductService
            _productService = new HttpProductService(ApiClientFactory.GetHttpClient());
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
            // Hiển thị ảnh hiện tại hoặc placeholder "Thêm ảnh"
            if (_currentImageIndex >= 0 && _currentImageIndex < _imageUrls.Count)
            {
                string currentUrl = _imageUrls[_currentImageIndex];
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
            }
            else
            {
                // Nếu index == Count, đang ở vị trí placeholder "Thêm ảnh"
                CurrentImage.Source = null;
            }

            // Cập nhật thumbnails
            UpdateThumbnails();

            // Vô hiệu hóa/kích hoạt nút điều hướng
            // Có thể điều hướng đến cả placeholder (index == Count)
            PrevButton.IsEnabled = _currentImageIndex > 0;
            NextButton.IsEnabled = _currentImageIndex < _imageUrls.Count;
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
                    var grid = CreateImageThumbnail(i);
                    ThumbnailsPanel.Children.Add(grid);
                }
            }

            // Thêm nút "+" để thêm ảnh mới (luôn hiện sau ảnh cuối cùng)
            var addBorder = CreateAddImageThumbnail();
            ThumbnailsPanel.Children.Add(addBorder);
        }

        /// <summary>
        /// Tạo thumbnail cho ảnh
        /// </summary>
        private Grid CreateImageThumbnail(int index)
        {
            var grid = new Grid
            {
                Width = 60,
                Height = 60,
                Margin = new Thickness(0, 0, 4, 0)
            };

            var border = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = index == _currentImageIndex 
                    ? new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue) 
                    : new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke),
                Tag = index
            };

            var image = new Image
            {
                Stretch = Microsoft.UI.Xaml.Media.Stretch.UniformToFill
            };

            try
            {
                image.Source = new BitmapImage(new Uri(_imageUrls[index]));
            }
            catch
            {
                image.Source = null;
            }

            border.Child = image;
            border.Tapped += OnThumbnailTapped;
            ToolTipService.SetToolTip(border, $"Click để chuyển đến ảnh {index + 1}");

            grid.Children.Add(border);

            // Nút xóa (X) ở góc trên bên phải
            var deleteButton = new Button
            {
                Content = "✕",
                Width = 24,
                Height = 24,
                FontSize = 12,
                Padding = new Thickness(0),
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(200, 255, 0, 0)),
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                CornerRadius = new CornerRadius(12),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, -8, -8, 0),
                Tag = index
            };

            deleteButton.Click += OnDeleteImageButtonClick;
            ToolTipService.SetToolTip(deleteButton, $"Xóa ảnh {index + 1}");

            grid.Children.Add(deleteButton);
            return grid;
        }

        /// <summary>
        /// Tạo thumbnail "Thêm ảnh"
        /// </summary>
        private Border CreateAddImageThumbnail()
        {
            var addBorder = new Border
            {
                Width = 60,
                Height = 60,
                BorderThickness = new Thickness(2),
                BorderBrush = _currentImageIndex == _imageUrls.Count
                    ? new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue)
                    : new SolidColorBrush(Microsoft.UI.Colors.Gray),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Microsoft.UI.Colors.WhiteSmoke),
                Tag = _imageUrls.Count // Tag = số lượng ảnh hiện tại
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
            addBorder.Tapped += OnAddImageThumbnailTapped;
            ToolTipService.SetToolTip(addBorder, $"Thêm ảnh mới (Hiện có: {_imageUrls.Count})");

            return addBorder;
        }

        /// <summary>
        /// Click vào thumbnail "Thêm ảnh" → Upload ảnh trực tiếp
        /// </summary>
        private async void OnAddImageThumbnailTapped(object sender, TappedRoutedEventArgs e)
        {
            await UploadNewImageAsync();
        }

        /// <summary>
        /// Click vào nút X để xóa ảnh
        /// </summary>
        private async void OnDeleteImageButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int index)
            {
                if (index >= 0 && index < _imageUrls.Count)
                {
                    var imageUrl = _imageUrls[index];
                    
                    // Gọi API để xóa ảnh trên server
                    try
                    {
                        var httpClient = ApiClientFactory.GetHttpClient();
                        var deleteRequest = new { imageUrl = imageUrl };
                        await httpClient.PostAsJsonAsync("api/Image", deleteRequest);
                    }
                    catch
                    {
                        // Silent fail
                    }
                    
                    // Xóa khỏi list
                    _imageUrls.RemoveAt(index);
                    
                    // Điều chỉnh current index nếu cần
                    if (_currentImageIndex >= _imageUrls.Count && _imageUrls.Count > 0)
                    {
                        _currentImageIndex = _imageUrls.Count - 1;
                    }
                    else if (_imageUrls.Count == 0)
                    {
                        _currentImageIndex = 0;
                    }
                    
                    UpdateCarouselUI();
                }
            }
        }

        /// <summary>
        /// Click vào thumbnail để chuyển đến ảnh đó
        /// </summary>
        private void OnThumbnailTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Border border && border.Tag is int index)
            {
                if (index >= 0 && index < _imageUrls.Count)
                {
                    _currentImageIndex = index;
                    UpdateCarouselUI();
                }
            }
        }

        /// <summary>
        /// Click vào ảnh chính → nếu đang ở placeholder thì mở file picker
        /// </summary>
        private async void OnMainImageTapped(object sender, TappedRoutedEventArgs e)
        {
            // Nếu đang ở vị trí placeholder (index == Count), cho phép thêm ảnh
            if (_currentImageIndex == _imageUrls.Count)
            {
                await UploadNewImageAsync();
            }
            // Nếu đang xem ảnh thật, không làm gì (hoặc có thể xem preview nếu muốn)
        }

        /// <summary>
        /// Click vào nút "+" để thêm ảnh mới
        /// </summary>
        private async void OnAddImageTapped(object sender, TappedRoutedEventArgs e)
        {
            await UploadNewImageAsync();
        }

        /// <summary>
        /// Upload ảnh mới
        /// </summary>
        private async Task UploadNewImageAsync()
        {
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
                            _imageUrls.Add(result.Data);
                            _currentImageIndex = _imageUrls.Count - 1;
                            UpdateCarouselUI();
                        }
                        else
                        {
                            await ShowErrorAsync("Lỗi Upload", result.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync("Lỗi ngoại lệ", ex.Message);
                }
            }
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
            if (_currentImageIndex < _imageUrls.Count)
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
            catch (Exception)
            {
                // Ignore errors while loading brands
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

            // Reset image list
            _imageUrls.Clear();

            // Load main image
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            {
                _imageUrls.Add(product.ImageUrl);
            }

            // Load gallery images
            if (!string.IsNullOrWhiteSpace(product.ImageGalleryJson))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(product.ImageGalleryJson);
                    if (list != null)
                    {
                        _imageUrls.AddRange(list.Where(url => !string.IsNullOrWhiteSpace(url)));
                    }
                }
                catch (Exception)
                {
                    // Ignore errors while parsing gallery images
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

            // Ảnh chính là ảnh đầu tiên
            string? mainImageUrl = _imageUrls.Count > 0 ? _imageUrls[0] : null;
            
            // Ảnh gallery là các ảnh còn lại
            var galleryList = _imageUrls.Skip(1).Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
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
    }
}
