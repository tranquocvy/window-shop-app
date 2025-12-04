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
using System.Collections.Generic;
using System;
using System.Diagnostics;
using LiveChartsCore.Measure;
using System.Collections.Specialized;
using LiveChartsCore.SkiaSharpView;

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
            // Placeholder: call LoadReportsAsync to ensure data is current, then export logic
            await ViewModel.LoadReportsCommand.ExecuteAsync(null);

            // TODO: implement export (PDF/Excel) using existing services
            var dialog = new ContentDialog
            {
                Title = "Xuất báo cáo",
                Content = "Chức năng xuất báo cáo sẽ được triển khai.",
                CloseButtonText = "Đóng",
                XamlRoot = this.Content.XamlRoot
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

            await tcs.Task;
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
