using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Http;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    /// <summary>
    /// ViewModel for the Dashboard page
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        #region Fields

        private readonly IDashboardService _service;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DashboardViewModel with default HTTP service
        /// </summary>
        public DashboardViewModel() : this(new HttpDashboardService(ApiClientFactory.GetHttpClient())) { }

        /// <summary>
        /// Initializes a new instance of DashboardViewModel with specified service
        /// </summary>
        /// <param name="service">Dashboard service instance</param>
        /// <exception cref="ArgumentNullException">Thrown when service is null</exception>
        public DashboardViewModel(IDashboardService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            LowStockProducts = new ObservableCollection<LowStockProductDto>();
            TopSellingProducts = new ObservableCollection<TopSellingProductDto>();
            RecentOrders = new ObservableCollection<RecentOrderDto>();
            MonthlyRevenue = new ObservableCollection<DailyRevenueDto>();
            
            InitializeChartSeries();
        }

        #endregion

        #region Observable Properties

        /// <summary>
        /// Total number of products in inventory
        /// </summary>
        [ObservableProperty]
        private int totalProducts;

        /// <summary>
        /// Total number of orders
        /// </summary>
        [ObservableProperty]
        private int totalOrders;

        /// <summary>
        /// Number of orders placed today
        /// </summary>
        [ObservableProperty]
        private int todayOrderCount;

        /// <summary>
        /// Revenue generated today (formatted as currency)
        /// </summary>
        [ObservableProperty]
        private string todayRevenue = string.Empty;

        /// <summary>
        /// Chart series for monthly revenue
        /// </summary>
        [ObservableProperty]
        private IEnumerable<ISeries> series = Array.Empty<ISeries>();

        /// <summary>
        /// X-axis configuration for the chart
        /// </summary>
        [ObservableProperty]
        private IEnumerable<Axis> xAxes = Array.Empty<Axis>();

        /// <summary>
        /// Y-axis configuration for the chart
        /// </summary>
        [ObservableProperty]
        private IEnumerable<Axis> yAxes = Array.Empty<Axis>();

        #endregion

        #region Collections

        /// <summary>
        /// Collection of products with low stock levels
        /// </summary>
        public ObservableCollection<LowStockProductDto> LowStockProducts { get; }

        /// <summary>
        /// Collection of top selling products
        /// </summary>
        public ObservableCollection<TopSellingProductDto> TopSellingProducts { get; }

        /// <summary>
        /// Collection of recent orders
        /// </summary>
        public ObservableCollection<RecentOrderDto> RecentOrders { get; }

        /// <summary>
        /// Collection of daily revenue data for the month
        /// </summary>
        public ObservableCollection<DailyRevenueDto> MonthlyRevenue { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads dashboard data asynchronously from the service
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task LoadAsync()
        {
            var response = await _service.GetDashboardAsync();
            
            if (response?.Success != true || response.Data == null)
            {
                return;
            }

            var data = response.Data;

            // Update summary metrics
            TotalProducts = data.TotalProducts;
            TodayOrderCount = data.TodayOrderCount;
            TodayRevenue = data.TodayRevenue.ToString("C");

            // Update collections
            UpdateCollection(LowStockProducts, data.LowStockProducts);
            UpdateCollection(TopSellingProducts, data.TopSellingProducts);
            UpdateCollection(RecentOrders, data.RecentOrders);
            UpdateCollection(MonthlyRevenue, data.MonthlyRevenue);
            
            // Update chart
            UpdateChartData();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes chart series with default configuration
        /// </summary>
        private void InitializeChartSeries()
        {
            Series = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = new List<decimal>(),
                    GeometrySize = 6,
                    GeometryStroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 2 },
                    Stroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 3 },
                    LineSmoothness = 0.8
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new List<string>(),
                    LabelsRotation = 0,
                    TextSize = 11,
                    SeparatorsPaint = null
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 11,
                    Labeler = value => value.ToString("C0"),
                    SeparatorsPaint = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 1 }
                }
            };
        }

        /// <summary>
        /// Updates chart data from MonthlyRevenue collection
        /// </summary>
        private void UpdateChartData()
        {
            if (MonthlyRevenue == null || MonthlyRevenue.Count == 0)
            {
                return;
            }

            // Update series values
            var revenueValues = MonthlyRevenue.Select(x => x.Revenue).ToList();
            Series = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = revenueValues,
                    GeometrySize = 6, // Size point
                    GeometryStroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 2 },
                    Stroke = new SolidColorPaint(SKColors.LightSkyBlue) { StrokeThickness = 3 },
                    LineSmoothness = 0.8
                }
            };

            // Update X-axis labels
            var dateLabels = MonthlyRevenue.Select(x => x.Date.ToString("dd/MM")).ToList();
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = dateLabels,
                    LabelsRotation = 0,
                    TextSize = 11,
                    SeparatorsPaint = null
                }
            };
        }

        /// <summary>
        /// Updates an observable collection with new items
        /// </summary>
        private static void UpdateCollection<T>(ObservableCollection<T> collection, IEnumerable<T>? items)
        {
            collection.Clear();
            
            if (items == null) return;

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        #endregion
    }
}
