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
            "S?n Ph?m",
            "Doanh Thu"
        };

        // Period Type Options
        public ObservableCollection<string> PeriodTypes { get; } = new()
        {
            "Ngày",
            "Tu?n", 
            "Tháng",
            "N?m"
        };

        // Products list for dropdown
        public ObservableCollection<ProductSummaryDto> Products { get; } = new();

        [ObservableProperty]
        private string _selectedChartTab = "S?n Ph?m"; // Default: Product chart

        [ObservableProperty]
        private ProductSummaryDto _selectedProduct;

        [ObservableProperty]
        private string _selectedPeriodType = "N?m";

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now; // Default: Hôm nay

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now; // Default: Hôm nay

        // Chart data for display
        public ObservableCollection<ProductSalesDto> ProductSalesData { get; } = new();
        public ObservableCollection<SalesReportDto> RevenueData { get; } = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage;

        // Computed properties for visibility
        public bool IsProductChartVisible => SelectedChartTab == "S?n Ph?m";
        public bool IsRevenueChartVisible => SelectedChartTab == "Doanh Thu";

        public ReportViewModel(IReportService reportService = null)
        {
            _reportService = reportService ?? CreateDefaultReportService();
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
                    // Add "All Products" option
                    Products.Add(new ProductSummaryDto { ProductId = 0, ProductName = "T?t c? s?n ph?m" });
                    
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
                var query = new ReportQueryDto
                {
                    StartDate = StartDate.DateTime,
                    EndDate = EndDate.DateTime,
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
                ErrorMessage = $"L?i t?i báo cáo: {ex.Message}";
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
