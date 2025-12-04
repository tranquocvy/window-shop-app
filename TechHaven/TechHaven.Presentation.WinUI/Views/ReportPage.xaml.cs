using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TechHaven.Presentation.WinUI.ViewModel;
using System.Linq;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Threading;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Reports;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using LiveChartsCore.Measure;
using System.Collections.Specialized;
using LiveChartsCore.SkiaSharpView;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class ReportPage : Page
    {
        public ReportViewModel ViewModel { get; }
        private readonly IReportService _reportService;
        private readonly IProductService _productService;

        public ReportPage()
        {
            this.InitializeComponent();
            ViewModel = new ReportViewModel();
            this.DataContext = ViewModel;
            _reportService = new HttpReportService(ApiClientFactory.GetHttpClient());
            _productService = new HttpProductService(ApiClientFactory.GetHttpClient());
            this.Loaded += ReportPage_Loaded;

            // subscribe to label changes so X axis updates when VM populates ChartLabels
            ViewModel.ChartLabels.CollectionChanged += ChartLabels_CollectionChanged;
        }

        private void ChartLabels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateChartLabels();
        }

        private async void ReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.LoadReportsCommand.ExecuteAsync(null);

                // If still no datapoints, notify the user to check API/backend
                if (ViewModel.RevenueData == null || ViewModel.RevenueData.Count == 0)
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Không có dữ liệu",
                        Content = $"Không có dữ liệu báo cáo. Kiểm tra API đang chạy tại: {AppState.ApiBaseUri} và đảm bảo endpoint /api/Report/sales trả về dữ liệu.",
                        CloseButtonText = "Đóng",
                        XamlRoot = this.Content.XamlRoot
                    };

                    var op = dialog.ShowAsync();
                    var tcs = new TaskCompletionSource<ContentDialogResult>();
                    op.Completed = (info, status) =>
                    {
                        try
                        {
                            var res = info.GetResults();
                            tcs.TrySetResult(res);
                        }
                        catch (System.Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    };

                    await tcs.Task;
                }
                else
                {
                    // ensure chart X axis is updated with period labels
                    UpdateChartLabels();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReportPage_Loaded error: {ex.Message}");
            }
        }

        private void UpdateChartLabels()
        {
            try
            {
                if (RevenueChart == null) return;

                var labels = ViewModel.ChartLabels?.ToList() ?? new List<string>();

                RevenueChart.XAxes = new LiveChartsCore.SkiaSharpView.Axis[]
                {
                    new LiveChartsCore.SkiaSharpView.Axis { Labels = labels }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateChartLabels error: {ex.Message}");
            }
        }

        private async void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            // Ensure latest data
            await ViewModel.LoadReportsCommand.ExecuteAsync(null);

            // Build export query from UI selections
            var query = new ReportQueryDto
            {
                StartDate = ViewModel.StartDate.DateTime,
                EndDate = ViewModel.EndDate.DateTime,
                PeriodType = ViewModel.SelectedPeriodType switch
                {
                    "Ngày" => ReportPeriodType.Daily,
                    "Tuần" => ReportPeriodType.Weekly,
                    "Tháng" => ReportPeriodType.Monthly,
                    "Năm" => ReportPeriodType.Yearly,
                    _ => ReportPeriodType.Monthly
                }
            };

            try
            {
                byte[] bytes = Array.Empty<byte>();
                string suggestedName = "report";

                if (ViewModel.SelectedChartTab == "Sản Phẩm")
                {
                    if (ViewModel.SelectedProduct == null || ViewModel.SelectedProduct.ProductId <= 0)
                    {
                        var warn = new ContentDialog { Title = "Chưa chọn sản phẩm", Content = "Vui lòng chọn sản phẩm trước khi xuất báo cáo sản phẩm.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                        await warn.ShowAsync();
                        return;
                    }

                    bytes = await _reportService.ExportProductAsync(ViewModel.SelectedProduct.ProductId, query);
                    suggestedName = $"product_{ViewModel.SelectedProduct.ProductId}_{ViewModel.StartDate:yyyyMMdd}_{ViewModel.EndDate:yyyyMMdd}";
                }
                else if (ViewModel.SelectedChartTab == "Hoa Hồng")
                {
                    // only full admin should access commission tab but validate
                    var commissionQuery = new CommissionQueryDto { Month = ViewModel.StartDate.Month, Year = ViewModel.StartDate.Year };
                    bytes = await _reportService.ExportCommissionAsync(commissionQuery);
                    suggestedName = $"commission_{commissionQuery.Month}_{commissionQuery.Year}";
                }
                else // default: sales
                {
                    bytes = await _reportService.ExportSalesAsync(query);
                    suggestedName = $"sales_{ViewModel.StartDate:yyyyMMdd}_{ViewModel.EndDate:yyyyMMdd}";
                }

                if (bytes == null || bytes.Length == 0)
                {
                    var err = new ContentDialog { Title = "Lỗi", Content = "Không có dữ liệu xuất hoặc API trả về lỗi.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                    await err.ShowAsync();
                    return;
                }

                // Show save file picker
                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("Excel workbook", new List<string> { ".xlsx" });
                picker.SuggestedFileName = suggestedName;

                // Try to initialize picker with window handle. Some environments (or timing) may return an invalid handle
                // causing "Invalid window handle" errors. Only call InitializeWithWindow if we have a non-zero handle.
                try
                {
                    if (App.MainWindow != null)
                    {
                        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                        if (hwnd != IntPtr.Zero)
                        {
                            InitializeWithWindow.Initialize(picker, hwnd);
                        }
                        else
                        {
                            Debug.WriteLine("Export: window handle is IntPtr.Zero, skipping InitializeWithWindow fallback to default picker.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Export: App.MainWindow is null, skipping InitializeWithWindow.");
                    }
                }
                catch (Exception ex)
                {
                    // Log and continue — fallback to calling PickSaveFileAsync without initialization
                    Debug.WriteLine($"InitializeWithWindow failed: {ex.Message}");
                }

                var file = await picker.PickSaveFileAsync();
                if (file == null)
                {
                    // user cancelled
                    return;
                }

                await FileIO.WriteBytesAsync(file, bytes);

                var ok = new ContentDialog { Title = "Hoàn thành", Content = "File đã được lưu.", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                await ok.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Export error: {ex.Message}");
                var err = new ContentDialog { Title = "Lỗi", Content = $"Xuất báo cáo thất bại: {ex.Message}", CloseButtonText = "Đóng", XamlRoot = this.Content.XamlRoot };
                await err.ShowAsync();
            }
        }

        private CancellationTokenSource _ctsSearch;

        private async void OpenProductPicker_Click(object sender, RoutedEventArgs e)
        {
            // Create search box and list
            // Make the search box width match the product name display area (240)
            var searchBox = new TextBox { PlaceholderText = "Tìm sản phẩm...", Width = 420, HorizontalAlignment = HorizontalAlignment.Left };
            var listView = new ListView { MaxHeight = 360, Width = 420, IsItemClickEnabled = true };

            // bind initial items (map ProductSummaryDto list)
            listView.ItemsSource = ViewModel.Products;
            listView.SelectionMode = ListViewSelectionMode.Single;

            // template
            listView.ItemTemplate = (DataTemplate)Resources["ProductListItemTemplate"];

            // filter logic -> debounce and call product Query API
            searchBox.TextChanged += async (_, _) =>
            {
                _ctsSearch?.Cancel();
                _ctsSearch = new CancellationTokenSource();
                var token = _ctsSearch.Token;

                // small debounce
                try
                {
                    await Task.Delay(300, token);
                }
                catch (TaskCanceledException) { return; }

                var kw = searchBox.Text?.Trim();
                try
                {
                    var query = new ProductListQueryDto
                    {
                        SearchTerm = kw,
                        PageNumber = 1,
                        PageSize = 50
                    };

                    var resp = await _productService.QueryProductsAsync(query);
                    if (token.IsCancellationRequested) return;

                    if (resp?.Success == true && resp.Data?.Items != null)
                    {
                        // map ProductDto -> ProductSummaryDto
                        var mapped = resp.Data.Items.Select(p => new ProductSummaryDto { ProductId = p.ProductId, ProductName = p.ProductName ?? string.Empty }).ToList();
                        listView.ItemsSource = mapped;
                    }
                    else
                    {
                        listView.ItemsSource = new List<ProductSummaryDto>();
                    }
                }
                catch
                {
                    // ignore errors
                    listView.ItemsSource = new List<ProductSummaryDto>();
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(searchBox);
            stack.Children.Add(new TextBlock { Text = "", Height = 6 });
            stack.Children.Add(listView);

            var dialog = new ContentDialog
            {
                Title = "Chọn sản phẩm",
                Content = stack,
                XamlRoot = this.Content.XamlRoot,
                PrimaryButtonText = "Chọn",
                CloseButtonText = "Đóng"
            };

            ProductSummaryDto selected = null;
            listView.ItemClick += (_, args) =>
            {
                selected = args.ClickedItem as ProductSummaryDto;
                dialog.Hide();
            };

            // Await WinRT IAsyncOperation using Completed -> TaskCompletionSource
            var op = dialog.ShowAsync();
            var tcs = new TaskCompletionSource<ContentDialogResult>();
            op.Completed = (info, status) =>
            {
                try
                {
                    var res = info.GetResults();
                    tcs.TrySetResult(res);
                }
                catch (System.Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            var result = await tcs.Task;

            _ctsSearch?.Cancel();

            if (selected != null)
            {
                ViewModel.SelectedProduct = selected;
            }
            else if (result == ContentDialogResult.Primary)
            {
                // try primary selection
                var sel = listView.SelectedItem as ProductSummaryDto;
                if (sel != null)
                {
                    ViewModel.SelectedProduct = sel;
                }
            }
        }

        private void ProductChartTab_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ChartTabs.Any())
            {
                ViewModel.SelectedChartTab = ViewModel.ChartTabs.First();
            }
        }

        private void RevenueChartTab_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ChartTabs.Count > 1)
            {
                ViewModel.SelectedChartTab = ViewModel.ChartTabs[1];
            }
        }

        private void CommissionTab_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedChartTab = "Hoa Hồng";
        }
    }
}
