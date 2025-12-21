using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Views.Controls; 
using TechHaven.Shared.DTOs.Products;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

            // Ẩn cột Giá Nhập nếu không phải Admin
            UpdateCostPriceColumnVisibility();
        }

        private void UpdateCostPriceColumnVisibility()
        {
            if (!ViewModel.IsAdmin)
            {
                Col_CostPrice_Header.Width = new GridLength(0);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                ViewModel.PageSize = AppState.PageSize;
            }
            catch { }

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
                        // 1. Lấy link ảnh gốc từ UserControl (Property bạn vừa tạo ở bước trước)
                        string? oldImage = productForm.OriginalImageUrl;
                        string? newImage = resultDto.ImageUrl;

  

                        // 2. So sánh: Nếu có ảnh cũ VÀ ảnh mới khác ảnh cũ -> Xóa ảnh cũ trên server
                        if (!string.IsNullOrEmpty(oldImage) && oldImage != newImage)
                        {
                            try
                            {
                                // Khởi tạo Service để gọi API Delete (giống cách làm trong UserControl)
                                var service = new HttpProductService(ApiClientFactory.GetHttpClient());
                                await service.DeleteImageAsync(oldImage);
                            }
                            catch (Exception ex)
                            {
                                // Log lỗi nếu cần, nhưng không chặn luồng update
                                System.Diagnostics.Debug.WriteLine($"Lỗi xóa ảnh cũ: {ex.Message}");
                            }
                        }
                        // Chế độ SỬA
                        await ViewModel.UpdateProductAsync(itemForEdit.Product.ProductId, resultDto);
                    }
                }
            };

            // 4. Hiển thị Dialog và xử lý nút HỦY => lưu draft
            var result = await dialog.ShowAsync();

            // If user clicked Close/Cancel (result == None), save as draft
            if (result == ContentDialogResult.None)
            {
                try
                {
                    // Build DTO from form without strict validation (draft)
                    var draftDto = productForm.GetFormData();
                    if (draftDto != null)
                    {
                        draftDto.IsDraft = true;

                        if (itemForEdit == null)
                        {
                            await ViewModel.CreateProductAsync(draftDto);
                        }
                        else
                        {
                            await ViewModel.UpdateProductAsync(itemForEdit.Product.ProductId, draftDto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ShowProductDialogAsync] Failed to save draft: {ex}");
                }
            }
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

        private async void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            // Allow user to pick CSV file (Excel -> save as CSV recommended)
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".csv");
            picker.FileTypeFilter.Add(".txt");

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;

            try
            {
                // Read file text
                string text;
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                using (var sr = new StreamReader(stream.AsStreamForRead(), Encoding.UTF8))
                {
                    text = await sr.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    var dlgEmpty = new ContentDialog { Title = "File trống", Content = "File bạn chọn rỗng.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await dlgEmpty.ShowAsync();
                    return;
                }

                // Parse CSV: first non-empty line is header
                var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .ToArray();
                if (lines.Length < 2)
                {
                    var dlg = new ContentDialog { Title = "Dữ liệu không hợp lệ", Content = "CSV cần có header và ít nhất 1 dòng dữ liệu.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await dlg.ShowAsync();
                    return;
                }

                var header = ParseCsvLine(lines[0]);
                var headerMap = header.Select((h, i) => new { Name = h?.Trim().ToLowerInvariant(), Index = i })
                                      .ToDictionary(x => x.Name ?? string.Empty, x => x.Index);

                var products = new List<ProductUpsertRequest>();

                for (int r = 1; r < lines.Length; r++)
                {
                    var fields = ParseCsvLine(lines[r]);
                    if (fields.All(string.IsNullOrWhiteSpace)) continue;

                    string GetField(string key)
                    {
                        if (headerMap.TryGetValue(key.ToLowerInvariant(), out var idx) && idx >= 0 && idx < fields.Count)
                            return fields[idx]?.Trim() ?? string.Empty;
                        return string.Empty;
                    }

                    // Map fields (names expected in header)
                    var dto = new ProductUpsertRequest
                    {
                        ProductName = GetField("productname") != string.Empty ? GetField("productname") : GetField("product_name") ?? string.Empty,
                        BrandName = GetField("brandname") != string.Empty ? GetField("brandname") : GetField("brand_name"),
                        Color = string.IsNullOrWhiteSpace(GetField("color")) ? null : GetField("color"),
                        StorageCapacity = ParseIntNullable(GetField("storagecapacity") ?? GetField("storage_capacity")),
                        Processor = string.IsNullOrWhiteSpace(GetField("processor")) ? null : GetField("processor"),
                        ScreenSize = ParseDecimalNullable(GetField("screensize") ?? GetField("screen_size")),
                        BatteryCapacity = ParseIntNullable(GetField("batterycapacity") ?? GetField("battery_capacity")),
                        ImageUrl = string.IsNullOrWhiteSpace(GetField("imageurl") ?? GetField("image_url")) ? string.Empty : (GetField("imageurl") ?? GetField("image_url")),
                        ImageGalleryJson = string.IsNullOrWhiteSpace(GetField("imagegalleryjson") ?? GetField("image_gallery_json")) ? null : GetField("imagegalleryjson") ?? GetField("image_gallery_json"),
                        CostPrice = ParseDecimal(GetField("costprice") ?? GetField("cost_price")),
                        SellPrice = ParseDecimal(GetField("sellprice") ?? GetField("sell_price")),
                        StockQuantity = ParseInt(GetField("stockquantity") ?? GetField("stock_quantity")),
                        Description = string.IsNullOrWhiteSpace(GetField("description")) ? null : GetField("description"),
                        IsDraft = ParseBool(GetField("isdraft") ?? GetField("is_draft"))
                    };

                    // Minimal validation: product name and sell price required
                    if (string.IsNullOrWhiteSpace(dto.ProductName) || dto.SellPrice <= 0)
                    {
                        // skip invalid row - you may collect errors instead
                        continue;
                    }

                    products.Add(dto);
                }

                if (!products.Any())
                {
                    var dlgNoValid = new ContentDialog { Title = "Không có sản phẩm hợp lệ", Content = "Không tìm thấy dòng hợp lệ trong file để import.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await dlgNoValid.ShowAsync();
                    return;
                }

                // Ask user about options (skipDuplicates / validateBeforeInsert)
                // skipDuplicates = true: Skip duplicate products (no error), false: return errors for duplicates
                // validateBeforeInsert = true: Validate all first (atomic), false: insert valid ones, skip invalid
                var skipDuplicates = true;
                var validateBeforeInsert = false; // Allow partial import

                var payload = new ProductBulkCreateRequestDto
                {
                    Products = products,
                    SkipDuplicates = skipDuplicates,
                    ValidateBeforeInsert = validateBeforeInsert
                };

                // Debug: In ra request trước khi gửi
                System.Diagnostics.Debug.WriteLine("=== BULK IMPORT REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Total products to import: {products.Count}");
                System.Diagnostics.Debug.WriteLine($"SkipDuplicates: {skipDuplicates}");
                System.Diagnostics.Debug.WriteLine($"ValidateBeforeInsert: {validateBeforeInsert}");
                System.Diagnostics.Debug.WriteLine("\nProducts (detailed):");
                for (int i = 0; i < Math.Min(5, products.Count); i++)
                {
                    var p = products[i];
                    System.Diagnostics.Debug.WriteLine($"\n  [{i + 1}] {p.ProductName}");
                    System.Diagnostics.Debug.WriteLine($"      Brand: {p.BrandName}");
                    System.Diagnostics.Debug.WriteLine($"      Color: {p.Color ?? "(null)"}");
                    System.Diagnostics.Debug.WriteLine($"      Storage: {p.StorageCapacity?.ToString() ?? "(null)"} GB");
                    System.Diagnostics.Debug.WriteLine($"      Processor: {p.Processor ?? "(null)"}");
                    System.Diagnostics.Debug.WriteLine($"      Screen: {p.ScreenSize?.ToString() ?? "(null)"} inches");
                    System.Diagnostics.Debug.WriteLine($"      Battery: {p.BatteryCapacity?.ToString() ?? "(null)"} mAh");
                    System.Diagnostics.Debug.WriteLine($"      ImageUrl: {(string.IsNullOrEmpty(p.ImageUrl) ? "(empty)" : p.ImageUrl)}");
                    System.Diagnostics.Debug.WriteLine($"      ImageGalleryJson: {p.ImageGalleryJson ?? "(null)"}");
                    System.Diagnostics.Debug.WriteLine($"      CostPrice: {p.CostPrice:N0} VND");
                    System.Diagnostics.Debug.WriteLine($"      SellPrice: {p.SellPrice:N0} VND");
                    System.Diagnostics.Debug.WriteLine($"      StockQuantity: {p.StockQuantity}");
                    System.Diagnostics.Debug.WriteLine($"      Description: {p.Description ?? "(null)"}");
                    System.Diagnostics.Debug.WriteLine($"      IsDraft: {p.IsDraft}");
                }
                if (products.Count > 5)
                {
                    System.Diagnostics.Debug.WriteLine($"\n  ... and {products.Count - 5} more products");
                }
                System.Diagnostics.Debug.WriteLine("\n===========================\n");

                var http = ApiClientFactory.GetHttpClient();

                // send request
                var resp = await http.PostAsJsonAsync("api/Product/bulk", payload);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    var dlgFail = new ContentDialog { Title = "Import thất bại", Content = $"Server returned {(int)resp.StatusCode}: {body}", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await dlgFail.ShowAsync();
                    return;
                }

                // Parse response using DTO
                try
                {
                    var wrapper = await resp.Content.ReadFromJsonAsync<TechHaven.Shared.DTOs.Common.ResponseWrapper<ProductBulkCreateResponseDto>>();

                    if (wrapper == null || wrapper.Data == null)
                    {
                        var dlg = new ContentDialog { Title = "Lỗi", Content = "Không thể đọc phản hồi từ server", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                        await dlg.ShowAsync();
                        return;
                    }

                    var data = wrapper.Data;

                    // Build a clear, user-facing message
                    string dialogContent;
                    if (wrapper.Success)
                    {
                        if (data.SuccessCount > 0)
                        {
                            dialogContent = $"Đã import dữ liệu thành công ({data.SuccessCount}/{data.TotalProducts}).";
                            if (data.FailedCount > 0)
                            {
                                dialogContent += $" Có {data.FailedCount} dòng không được import do lỗi.";
                            }
                        }
                        else
                        {
                            dialogContent = "Import hoàn tất nhưng không có sản phẩm nào được thêm.";
                            if (data.FailedCount > 0)
                                dialogContent += $" Có {data.FailedCount} dòng lỗi.";
                        }
                    }
                    else
                    {
                        // If server returned partial results with errors, show concise errors
                        if (data.Errors != null && data.Errors.Any())
                        {
                            var firstErrors = data.Errors.Take(5)
                                .SelectMany(e => e.ErrorMessages)
                                .Take(10)
                                .ToArray();
                            var errorsText = string.Join("\n", firstErrors);
                            dialogContent = $"Import có lỗi. Hiển thị một vài lỗi:\n{errorsText}";
                            if (data.Errors.Sum(e => e.ErrorMessages.Count) > firstErrors.Length)
                                dialogContent += "\n...";
                        }
                        else
                        {
                            // fallback to wrapper message
                            dialogContent = wrapper.Message ?? "Import không thành công.";
                        }
                    }

                    var dlgOk = new ContentDialog
                    {
                        Title = wrapper.Success ? "Import hoàn tất" : "Import có lỗi",
                        Content = dialogContent,
                        CloseButtonText = "Đóng",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await dlgOk.ShowAsync();

                    // Refresh products if at least some succeeded
                    if (data.SuccessCount > 0)
                    {
                        ViewModel.LoadProductsCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    var dlg = new ContentDialog { Title = "Lỗi parse response", Content = $"Không thể xử lý phản hồi: {ex.Message}", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await dlg.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog { Title = "Lỗi", Content = ex.Message, CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                await dlg.ShowAsync();
            }
        }

        // ===================================================================
        // CSV PARSING HELPER METHODS
        // ===================================================================

        /// <summary>
        /// Parses a CSV line handling quoted fields with commas
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Check for escaped quote ("")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result;
        }

        /// <summary>
        /// Parse nullable integer from string
        /// </summary>
        private static int? ParseIntNullable(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(value.Trim(), out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Parse nullable decimal from string
        /// </summary>
        private static decimal? ParseDecimalNullable(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value.Trim(), out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Parse integer from string (defaults to 0)
        /// </summary>
        private static int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (int.TryParse(value.Trim(), out var result))
                return result;

            return 0;
        }

        /// <summary>
        /// Parse decimal from string (defaults to 0)
        /// </summary>
        private static decimal ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            if (decimal.TryParse(value.Trim(), out var result))
                return result;

            return 0m;
        }

        /// <summary>
        /// Parse boolean from string
        /// </summary>
        private static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var trimmed = value.Trim().ToLowerInvariant();
            return trimmed == "true" || trimmed == "1" || trimmed == "yes" || trimmed == "có";
        }

        private async void AddBrand_Click(object sender, RoutedEventArgs e)
        {
            // Tạo TextBox để nhập tên hãng
            var brandInput = new TextBox
            {
                PlaceholderText = "Nhập tên hãng mới...",
                Width = 300
            };

            // Tạo dialog
            var dialog = new ContentDialog
            {
                Title = "Thêm thương hiệu mới",
                Content = brandInput,
                PrimaryButtonText = "Lưu",
                CloseButtonText = "Hủy",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();

            // Nếu người dùng bấm Lưu
            if (result == ContentDialogResult.Primary)
            {
                string brandName = brandInput.Text?.Trim();

                if (string.IsNullOrEmpty(brandName))
                {
                    // Hiển thị thông báo lỗi
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = "Tên hãng không được để trống",
                        CloseButtonText = "Đóng",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // Tạo sản phẩm ảo với IsDraft = true
                var draftProduct = new ProductUpsertRequest
                {
                    ProductName = $"ma_prroduct_{DateTime.Now:yyyyMMddHHmmss}",
                    BrandName = brandName,
                    SellPrice = 0,
                    CostPrice = 0,
                    StockQuantity = 0,
                    ImageUrl = string.Empty,
                    IsDraft = true
                };

                try
                {
                    // Gọi API tạo sản phẩm
                    await ViewModel.CreateProductAsync(draftProduct);

                    // Reload danh sách brands trong filter dropdown
                    await ViewModel.LoadBrandsAsync();

                    // Hiển thị thông báo thành công
                    var successDialog = new ContentDialog
                    {
                        Title = "Thành công",
                        Content = $"Đã thêm thương hiệu '{brandName}' vào hệ thống",
                        CloseButtonText = "Đóng",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();

                    // Reload danh sách sản phẩm
                    await ViewModel.LoadProductsAsync();
                }
                catch (Exception ex)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Lỗi",
                        Content = $"Không thể thêm thương hiệu: {ex.Message}",
                        CloseButtonText = "Đóng",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}