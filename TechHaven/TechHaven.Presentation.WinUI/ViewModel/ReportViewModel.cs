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

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

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

        // Years and Months for filtering
        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<string> Months { get; } = new();

        [ObservableProperty]
        private string _selectedChartTab = "Sản Phẩm"; // Default: Product chart

        [ObservableProperty]
        private ProductSummaryDto _selectedProduct;

        [ObservableProperty]
        private string _selectedPeriodType = "Năm";

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now; // Default: Hôm nay

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now; // Default: Hôm nay

        [ObservableProperty]
        private int _selectedYear;

        [ObservableProperty]
        private string _selectedMonth = "Tất cả"; // default to 'all'

        // Chart data for display
        public ObservableCollection<ProductSalesTrendDto> ProductSalesData { get; } = new();
        public ObservableCollection<SalesReportDto> RevenueData { get; } = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        // Role flags
        private bool _isAdmin; // indicates admin-like UI (both admin and seller see most report UI)
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
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
            set => SetProperty(ref _isSeller, value);
        }

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
        private decimal _totalCommission;

        public ReportViewModel(IReportService reportService = null)
        {
            _reportService = reportService ?? CreateDefaultReportService();
            InitializeYearMonth();

            // determine role from AppState.CurrentUser.RoleName
            var role = AppState.CurrentUser?.RoleName ?? string.Empty;

            IsSeller = role.Equals("Seller", StringComparison.OrdinalIgnoreCase) || role.Equals("Vendor", StringComparison.OrdinalIgnoreCase);
            IsFullAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

            // IsAdmin kept true for both sellers and admins so both can see main report UI
            IsAdmin = IsSeller || IsFullAdmin;

            _ = LoadProductsAsync();
        }

        private void InitializeYearMonth()
        {
            var currentYear = DateTime.Now.Year;
            for (int y = currentYear - 4; y <= currentYear; y++)
            {
                Years.Add(y);
            }

            // "Tất cả" means 'All'
            Months.Add("Tất cả");
            for (int m = 1; m <= 12; m++)
            {
                Months.Add(m.ToString());
            }

            SelectedYear = currentYear;
            SelectedMonth = "Tất cả"; // All months by default
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
                    
                    // Select first product by default if available
                    SelectedProduct = Products.FirstOrDefault();
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

                // If year/month selector used, override dates accordingly
                if (SelectedYear > 0)
                {
                    if (!string.IsNullOrWhiteSpace(SelectedMonth) && SelectedMonth != "Tất cả")
                    {
                        if (int.TryParse(SelectedMonth, out var month))
                        {
                            queryStart = new DateTime(SelectedYear, month, 1);
                            queryEnd = queryStart.AddMonths(1).AddDays(-1);
                        }
                    }
                    else
                    {
                        // whole year
                        queryStart = new DateTime(SelectedYear, 1, 1);
                        queryEnd = new DateTime(SelectedYear, 12, 31);
                    }
                }

                var query = new ReportQueryDto
                {
                    StartDate = queryStart,
                    EndDate = queryEnd,
                    PeriodType = MapPeriodType(SelectedPeriodType)
                };

                // If commission tab selected and user is full admin, load commission data
                if (IsCommissionVisible)
                {
                    await LoadCommissionAsync(query);
                    return;
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
                    foreach (var item in revenueTrend.DataPoints ?? new List<SalesReportDto>())
                    {
                        RevenueData.Add(item);
                    }

                    // compute summary values from trend summary
                    TotalRevenue = revenueTrend.Summary?.TotalRevenue ?? 0m;
                    TotalProfit = revenueTrend.Summary?.Profit ?? 0m;

                    // format growth e.g. "[^ 15.5% Growth]"
                    RevenueGrowthText = revenueTrend.RevenueGrowth != 0m
                        ? $"[^ {revenueTrend.RevenueGrowth:P1} Growth]"
                        : "[^ 0.0% Growth]";

                    // if seller, compute simple metrics from summary
                    if (IsSeller)
                    {
                        SellerOrderCount = revenueTrend.Summary?.TotalOrders ?? 0;
                        SellerCommission = Math.Round((revenueTrend.Summary?.TotalRevenue ?? 0m) * 0.10m, 2);
                    }
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

        [RelayCommand]
        private async Task LoadCommissionAsync(ReportQueryDto query)
        {
            try
            {
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

        partial void OnSelectedPeriodTypeChanged(string value)
        {
            _ = LoadReportsAsync();
        }

        partial void OnSelectedProductChanged(ProductSummaryDto value)
        {
            _ = LoadReportsAsync();
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
