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
            "Doanh Thu"
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
        private string _selectedMonth; // "Tất cả" == all months

        // Chart data for display
        public ObservableCollection<ProductSalesDto> ProductSalesData { get; } = new();
        public ObservableCollection<SalesReportDto> RevenueData { get; } = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        // Computed properties for visibility
        public bool IsProductChartVisible => SelectedChartTab == "Sản Phẩm";
        public bool IsRevenueChartVisible => SelectedChartTab == "Doanh Thu";

        public ReportViewModel(IReportService reportService = null)
        {
            _reportService = reportService ?? CreateDefaultReportService();
            InitializeYearMonth();
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
                    // Add "All Products" option
                    Products.Add(new ProductSummaryDto { ProductId = 0, ProductName = "Tất cả sản phẩm" });
                    
                    foreach (var product in products)
                    {
                        Products.Add(product);
                    }
                    
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
                    PeriodType = MapPeriodType(SelectedPeriodType),
                    UserId = null // For now, show all users
                };

                // Load both reports
                var productSalesTask = _reportService.GetProductSalesReportAsync(query);
                var revenueTask = _reportService.GetRevenueReportAsync(query);

                await Task.WhenAll(productSalesTask, revenueTask);

                var productSalesData = await productSalesTask;
                var revenueData = await revenueTask;

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

                if (revenueData != null)
                {
                    RevenueData.Clear();
                    foreach (var item in revenueData)
                    {
                        RevenueData.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"L?i t?i b�o c�o: {ex.Message}";
                Debug.WriteLine($"Error loading reports: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private ReportPeriodType MapPeriodType(string periodType)
        {
            return periodType switch
            {
                "Ngày" => ReportPeriodType.Daily,
                "Tu?n" => ReportPeriodType.Weekly,
                "Tháng" => ReportPeriodType.Monthly,
                "N?m" => ReportPeriodType.Yearly,
                _ => ReportPeriodType.Monthly
            };
        }

        partial void OnSelectedChartTabChanged(string value)
        {
            OnPropertyChanged(nameof(IsProductChartVisible));
            OnPropertyChanged(nameof(IsRevenueChartVisible));
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
