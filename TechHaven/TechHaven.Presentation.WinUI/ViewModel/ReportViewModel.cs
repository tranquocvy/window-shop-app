using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Reports;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

// LiveCharts imports for chart series
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using LiveChartsCore.Kernel.Sketches;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        [ObservableProperty]
        private string _commissionTitle = "Bảng hoa hồng nhân viên";

        // Tab Options
        public ObservableCollection<string> ChartTabs { get; } = new()
        {
            "Sản Phẩm",
            "Doanh Thu",
            // Note: "Hoa Hồng" tab is only visible to full admins via IsFullAdmin
        };

        // Period Type Options
        public ObservableCollection<string> PeriodTypes { get; } = new()
        {
            "Ngày",
            "Tuần", 
            "Tháng",
            "Năm"
        };

        // Products list for dropdown
        public ObservableCollection<ProductSummaryDto> Products { get; } = new();

        // Commission report data (admin only)
        public ObservableCollection<CommissionReportDto> CommissionData { get; } = new();

        [ObservableProperty]
        private string _selectedChartTab = "Sản Phẩm"; // Default: Product chart

        [ObservableProperty]
        private ProductSummaryDto _selectedProduct;

        public bool IsProductSelected => SelectedProduct != null && SelectedProduct.ProductId > 0;
        public bool IsProductNotSelected => !IsProductSelected;

        [ObservableProperty]
        private string _selectedPeriodType = "Ngày"; // Default: Daily period

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-6); // Default: last 7 days (inclusive)

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now; // Default: today

        // Chart data for display
        public ObservableCollection<ProductSalesTrendDto> ProductSalesData { get; } = new();
        public ObservableCollection<SalesReportDto> RevenueData { get; } = new();

        // New collections for chart binding: labels and two series (revenue, profit)
        public ObservableCollection<string> ChartLabels { get; } = new();
        public ObservableCollection<double> RevenueValues { get; } = new();
        public ObservableCollection<double> ProfitValues { get; } = new();

        // LiveCharts series property for binding in XAML
        private IEnumerable<ISeries> _chartSeries = Array.Empty<ISeries>();
        public IEnumerable<ISeries> ChartSeries
        {
            get => _chartSeries;
            set => SetProperty(ref _chartSeries, value);
        }

        // Product chart related
        public ProductSalesTrendDto SelectedProductDetail { get; } = new ProductSalesTrendDto();
        public ObservableCollection<string> ProductChartLabels { get; } = new();
        public ObservableCollection<double> ProductQuantityValues { get; } = new();

        [ObservableProperty]
        private string _productStockText = "—";

        [ObservableProperty]
        private string _productTotalSoldText = "0 cái";

        [ObservableProperty]
        private decimal _productTotalRevenueValue;

        private IEnumerable<ISeries> _productChartSeries = Array.Empty<ISeries>();
        public IEnumerable<ISeries> ProductChartSeries
        {
            get => _productChartSeries;
            set => SetProperty(ref _productChartSeries, value);
        }

        private ICartesianAxis[] _productXAxes = Array.Empty<ICartesianAxis>();
        public ICartesianAxis[] ProductXAxes
        {
            get => _productXAxes;
            set => SetProperty(ref _productXAxes, value);
        }

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        // Role flags
        private bool _isAdmin; // indicates admin-like UI (both admin and seller see most report UI)
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (SetProperty(ref _isAdmin, value))
                {
                    OnPropertyChanged(nameof(CanExport));
                }
            }
        }

        // Full admin (real admin) - can see commission tab
        private bool _isFullAdmin;
        public bool IsFullAdmin
        {
            get => _isFullAdmin;
            set => SetProperty(ref _isFullAdmin, value);
        }

        private bool _isSeller;
        public bool IsSeller
        {
            get => _isSeller;
            set
            {
                if (SetProperty(ref _isSeller, value))
                {
                    OnPropertyChanged(nameof(CanExport));
                }
            }
        }

        // Both admins and sellers can export reports (but commission export remains admin-only)
        public bool CanExport => IsAdmin || IsSeller;

        // Seller-only metrics
        private int _sellerOrderCount;
        public int SellerOrderCount
        {
            get => _sellerOrderCount;
            set => SetProperty(ref _sellerOrderCount, value);
        }

        private decimal _sellerCommission;
        public decimal SellerCommission
        {
            get => _sellerCommission;
            set => SetProperty(ref _sellerCommission, value);
        }

        // Computed properties for visibility
        public bool IsProductChartVisible => SelectedChartTab == "Sản Phẩm";
        public bool IsRevenueChartVisible => SelectedChartTab == "Doanh Thu";
        public bool IsCommissionVisible => SelectedChartTab == "Hoa Hồng" && IsFullAdmin;

        // Summary metrics
        [ObservableProperty]
        private decimal _totalRevenue;

        [ObservableProperty]
        private decimal _totalProfit;

        [ObservableProperty]
        private string _revenueGrowthText = string.Empty;

        [ObservableProperty]
        private string _profitGrowthText = string.Empty;

        [ObservableProperty]
        private SolidColorBrush _revenueGrowthBrush = new SolidColorBrush(Colors.Transparent);

        [ObservableProperty]
        private SolidColorBrush _profitGrowthBrush = new SolidColorBrush(Colors.Transparent);

        [ObservableProperty]
        private decimal _totalCommission;

        [ObservableProperty]
        private string _summaryPeriod = string.Empty;

        [ObservableProperty]
        private int _summaryTotalOrders;

        [ObservableProperty]
        private decimal _summaryTotalCost;

        [ObservableProperty]
        private decimal _summaryProfitMargin;

        [ObservableProperty]
        private decimal _profitGrowth;

        public ReportViewModel(IReportService reportService = null)
        {
            _reportService = reportService ?? CreateDefaultReportService();

            // determine role from AppState.CurrentUser.RoleName
            var role = AppState.CurrentUser?.RoleName ?? string.Empty;

            IsSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase) || role.Equals("Vendor", StringComparison.OrdinalIgnoreCase);
            IsFullAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

            // IsAdmin kept true for both sellers and admins so both can see main report UI
            IsAdmin = IsSeller || IsFullAdmin;

            _ = LoadProductsAsync();
        }

        private static IReportService CreateDefaultReportService()
        {
            var httpClient = ApiClientFactory.GetHttpClient();
            return new HttpReportService(httpClient);
        }

        [RelayCommand]
        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _reportService.GetProductsAsync();
                if (products != null)
                {
                    Products.Clear();
                    
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    
                    // Do not select first product by default - wait for user
                    // SelectedProduct = Products.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadReportsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                DateTime queryStart = StartDate.DateTime;
                DateTime queryEnd = EndDate.DateTime;

                // Filtering is based on StartDate/EndDate (set by UI)

                var query = new ReportQueryDto
                {
                    StartDate = queryStart,
                    EndDate = queryEnd,
                    PeriodType = MapPeriodType(SelectedPeriodType)
                };

                // If commission tab selected and user is full admin, load commission data
                if (IsCommissionVisible)
                {
                    // map to CommissionQueryDto using the query start date (month/year)
                    var commissionQuery = new TechHaven.Shared.DTOs.Reports.CommissionQueryDto
                    {
                        Month = queryStart.Month,
                        Year = queryStart.Year
                    };

                    await LoadCommissionAsync(commissionQuery);
                    return;
                }
                
                // If product chart visible, and a product is selected, fetch product detail and prepare line chart
                if (IsProductChartVisible)
                {
                    if (SelectedProduct == null || SelectedProduct.ProductId <= 0)
                    {
                        // clear chart
                        ProductChartLabels.Clear();
                        ProductQuantityValues.Clear();
                        ProductChartSeries = Array.Empty<ISeries>();
                        ProductXAxes = Array.Empty<ICartesianAxis>();
                    }
                    else
                    {
                        try
                        {
                            var prodQuery = new ReportQueryDto { StartDate = queryStart, EndDate = queryEnd, PeriodType = MapPeriodType(SelectedPeriodType) };
                            var detail = await _reportService.GetProductDetailAsync(SelectedProduct.ProductId, prodQuery);

                            ProductChartLabels.Clear();
                            ProductQuantityValues.Clear();
                            SelectedProductDetail.DataPoints.Clear();

                            foreach (var dp in (detail?.DataPoints ?? new List<ProductSalesDataPointDto>()).OrderBy(d => d.Period))
                            {
                                SelectedProductDetail.DataPoints.Add(dp);
                                ProductChartLabels.Add(dp.Period);
                                ProductQuantityValues.Add(dp.QuantitySold);
                            }

                            // populate summary fields from product detail
                            ProductTotalSoldText = $"{detail.TotalQuantitySold} cái";
                            ProductTotalRevenueValue = detail.TotalRevenue;
                            // stock not available in report DTO; keep placeholder
                            ProductStockText = "—";

                            ProductChartSeries = new ISeries[] { new LineSeries<double> { Values = ProductQuantityValues, Name = "Số lượng" } };
                            ProductXAxes = new ICartesianAxis[] { new Axis { Labels = ProductChartLabels } };
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading product detail: {ex.Message}");
                            ProductChartSeries = Array.Empty<ISeries>();
                            ProductXAxes = Array.Empty<ICartesianAxis>();
                        }
                    }
                }

                // Load both reports
                var productSalesTask = _reportService.GetProductSalesReportAsync(query);
                var revenueTask = _reportService.GetRevenueReportAsync(query);

                // fallback to proper calls
                if (productSalesTask == null)
                    productSalesTask = _reportService.GetProductSalesReportAsync(query);
                if (revenueTask == null)
                    revenueTask = _reportService.GetRevenueReportAsync(query);

                await Task.WhenAll(productSalesTask, revenueTask);

                var productSalesData = await productSalesTask;
                var revenueTrend = await revenueTask; // SalesTrendDto

                if (productSalesData != null)
                {
                    ProductSalesData.Clear();
                    
                    // Filter by selected product if not "All Products"
                    var filteredData = SelectedProduct?.ProductId > 0
                        ? productSalesData.Where(p => p.ProductId == SelectedProduct.ProductId).ToList()
                        : productSalesData;
                    
                    foreach (var item in filteredData)
                    {
                        ProductSalesData.Add(item);
                    }
                }

                if (revenueTrend != null)
                {
                    RevenueData.Clear();

                    // Clear chart series/labels before populating
                    ChartLabels.Clear();
                    RevenueValues.Clear();
                    ProfitValues.Clear();

                    foreach (var item in revenueTrend.DataPoints ?? new List<SalesReportDto>())
                    {
                        RevenueData.Add(item);

                        // period used as X axis label
                        ChartLabels.Add(item.Period);

                        // series values for chart (cast to double for most chart controls)
                        RevenueValues.Add((double)item.TotalRevenue);
                        ProfitValues.Add((double)item.Profit);
                    }

                    // create LiveCharts series
                    ChartSeries = new ISeries[]
                    {
                        new ColumnSeries<double> { Values = RevenueValues, Name = "Doanh thu" },
                        new ColumnSeries<double> { Values = ProfitValues, Name = "Lợi nhuận" }
                    };

                    // compute summary values from trend summary
                    TotalRevenue = revenueTrend.Summary?.TotalRevenue ?? 0m;
                    TotalProfit = revenueTrend.Summary?.Profit ?? 0m;

                    // revenue growth text - API already returns percent value; show as-is with % sign
                    RevenueGrowthText = revenueTrend.RevenueGrowth != 0m ? $"{revenueTrend.RevenueGrowth:F2}%" : "0.00%";
                    RevenueGrowthBrush = revenueTrend.RevenueGrowth > 0
                        ? new SolidColorBrush(Colors.Green)
                        : (revenueTrend.RevenueGrowth < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green));

                    // additional summary fields
                    SummaryPeriod = revenueTrend.Summary?.Period ?? string.Empty;
                    SummaryTotalOrders = revenueTrend.Summary?.TotalOrders ?? 0;
                    SummaryTotalCost = revenueTrend.Summary?.TotalCost ?? 0m;
                    SummaryProfitMargin = revenueTrend.Summary?.ProfitMargin ?? 0m;
                    ProfitGrowth = revenueTrend.ProfitGrowth;
                    // profit growth text - show raw percent
                    ProfitGrowthText = revenueTrend.ProfitGrowth != 0m ? $"{revenueTrend.ProfitGrowth:F2}%" : "0.00%";
                    ProfitGrowthBrush = revenueTrend.ProfitGrowth > 0
                        ? new SolidColorBrush(Colors.Green)
                        : (revenueTrend.ProfitGrowth < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi tới báo cáo: {ex.Message}";
                Debug.WriteLine($"Error loading reports: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCommissionAsync(TechHaven.Shared.DTOs.Reports.CommissionQueryDto query)
        {
            try
            {
                // update title to show month/year used for the commission query
                CommissionTitle = $"Bảng hoa hồng nhân viên T{query.Month}/{query.Year}";

                var data = await _reportService.GetCommissionReportAsync(query);
                CommissionData.Clear();
                foreach (var item in data)
                {
                    CommissionData.Add(item);
                }

                // compute total commission to pay
                TotalCommission = CommissionData.Sum(c => c.CommissionAmount);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading commission report: {ex.Message}");
            }
        }

        private ReportPeriodType MapPeriodType(string periodType)
        {
            return periodType switch
            {
                "Ngày" => ReportPeriodType.Daily,
                "Tuần" => ReportPeriodType.Weekly,
                "Tháng" => ReportPeriodType.Monthly,
                "Năm" => ReportPeriodType.Yearly,
                _ => ReportPeriodType.Monthly
            };
        }

        partial void OnSelectedChartTabChanged(string value)
        {
            OnPropertyChanged(nameof(IsProductChartVisible));
            OnPropertyChanged(nameof(IsRevenueChartVisible));
            OnPropertyChanged(nameof(IsCommissionVisible));
            _ = LoadReportsAsync();
        }

        // Do not auto-load when period type or selected product changes; require user to click Search
        partial void OnSelectedPeriodTypeChanged(string value) { }
        partial void OnSelectedProductChanged(ProductSummaryDto value)
        {
            OnPropertyChanged(nameof(IsProductSelected));
            OnPropertyChanged(nameof(IsProductNotSelected));
        }

        partial void OnStartDateChanged(DateTimeOffset value)
        {
            if (value > EndDate)
            {
                EndDate = value;
            }
        }

        partial void OnEndDateChanged(DateTimeOffset value)
        {
            if (value < StartDate)
            {
                StartDate = value;
            }
        }
    }
}
