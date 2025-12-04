using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
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
using Windows.Foundation;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    /// <summary>
    /// ViewModel for the Dashboard page
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        #region Fields

        private static readonly HttpClient SharedHttpClient = ApiClientFactory.GetHttpClient();
        private readonly IDashboardService _service;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DashboardViewModel with default HTTP service
        /// </summary>
        public DashboardViewModel() : this(new HttpDashboardService(SharedHttpClient)) { }

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
        /// Geometry path for desktop monthly revenue chart
        /// </summary>
        [ObservableProperty]
        private Geometry? monthlyRevenueGeometry;

        /// <summary>
        /// Geometry path for tablet monthly revenue chart
        /// </summary>
        [ObservableProperty]
        private Geometry? tabletMonthlyRevenueGeometry;

        /// <summary>
        /// Spline tension for chart smoothness (0 = linear, 1 = full Catmull-Rom)
        /// </summary>
        [ObservableProperty]
        private double splineTension = 1.0;

        /// <summary>
        /// Number of interpolation steps per segment for chart smoothness
        /// </summary>
        [ObservableProperty]
        private int splineSamplesPerSegment = 12;

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
        }

        /// <summary>
        /// Updates desktop chart geometry based on container dimensions
        /// </summary>
        /// <param name="width">Container width in pixels</param>
        /// <param name="height">Container height in pixels</param>
        public void UpdateMonthlyRevenueGeometry(double width, double height)
        {
            MonthlyRevenueGeometry = GenerateChartGeometry(width, height);
        }

        /// <summary>
        /// Updates tablet chart geometry based on container dimensions
        /// </summary>
        /// <param name="width">Container width in pixels</param>
        /// <param name="height">Container height in pixels</param>
        public void UpdateTabletMonthlyRevenueGeometry(double width, double height)
        {
            TabletMonthlyRevenueGeometry = GenerateChartGeometry(width, height);
        }

        #endregion

        #region Private Methods

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

        /// <summary>
        /// Generates chart geometry path for the given dimensions
        /// </summary>
        /// <param name="width">Chart container width</param>
        /// <param name="height">Chart container height</param>
        /// <returns>PathGeometry for the chart line, or null if invalid</returns>
        private Geometry? GenerateChartGeometry(double width, double height)
        {
            if (!IsValidChartDimensions(width, height) || !HasValidRevenueData())
            {
                return null;
            }

            var values = MonthlyRevenue.Select(x => x.Revenue).ToList();
            var (min, max, range) = CalculateValueRange(values);
            var points = CalculateChartPoints(values, min, max, range, width, height);

            if (points.Count == 1)
            {
                return null; // Single point - nothing to draw
            }

            var interpolatedPoints = InterpolateSplinePoints(points);
            return CreatePathGeometry(points[0], interpolatedPoints);
        }

        /// <summary>
        /// Validates chart dimensions
        /// </summary>
        private static bool IsValidChartDimensions(double width, double height)
        {
            return width > 0 && height > 0 && 
                   !double.IsNaN(width) && !double.IsNaN(height) &&
                   !double.IsInfinity(width) && !double.IsInfinity(height);
        }

        /// <summary>
        /// Checks if revenue data is available
        /// </summary>
        private bool HasValidRevenueData()
        {
            return MonthlyRevenue != null && MonthlyRevenue.Count > 0;
        }

        /// <summary>
        /// Calculates min, max, and range of values
        /// </summary>
        private static (decimal min, decimal max, decimal range) CalculateValueRange(List<decimal> values)
        {
            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min;
            
            if (range == 0) range = 1; // Avoid divide by zero

            return (min, max, range);
        }

        /// <summary>
        /// Calculates chart point coordinates
        /// </summary>
        private List<Point> CalculateChartPoints(List<decimal> values, decimal min, decimal max, decimal range, double width, double height)
        {
            int n = values.Count;
            double w = Math.Max(ChartConfiguration.MinChartDimension, 
                width - ChartConfiguration.LeftPadding - ChartConfiguration.RightPadding);
            double h = Math.Max(ChartConfiguration.MinChartDimension, 
                height - ChartConfiguration.TopPadding - ChartConfiguration.BottomPadding);

            var points = new List<Point>(n);
            
            for (int i = 0; i < n; i++)
            {
                double x = ChartConfiguration.LeftPadding + (n == 1 ? w / 2 : (w * i) / (n - 1));
                double normalized = (double)((values[i] - min) / range);
                double y = ChartConfiguration.TopPadding + (1 - normalized) * h;
                points.Add(new Point(x, y));
            }

            return points;
        }

        /// <summary>
        /// Interpolates points using Catmull-Rom spline
        /// </summary>
        private List<Point> InterpolateSplinePoints(List<Point> points)
        {
            int n = points.Count;
            int samples = Math.Max(1, splineSamplesPerSegment);
            double tension = Math.Clamp(splineTension, 0.0, 1.0);
            var interpolatedPoints = new List<Point>();

            for (int i = 0; i < n - 1; i++)
            {
                Point p0 = i > 0 ? points[i - 1] : points[i];
                Point p1 = points[i];
                Point p2 = points[i + 1];
                Point p3 = i + 2 < n ? points[i + 2] : points[i + 1];

                for (int s = 1; s <= samples; s++)
                {
                    double t = (double)s / samples;
                    var point = InterpolateCatmullRom(p0, p1, p2, p3, t, tension);
                    interpolatedPoints.Add(point);
                }
            }

            return interpolatedPoints;
        }

        /// <summary>
        /// Interpolates a single point using Catmull-Rom algorithm
        /// </summary>
        private static Point InterpolateCatmullRom(Point p0, Point p1, Point p2, Point p3, double t, double tension)
        {
            double t2 = t * t;
            double t3 = t2 * t;

            // Catmull-Rom spline
            double cr_x = 0.5 * ((2 * p1.X) + 
                (-p0.X + p2.X) * t + 
                (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 + 
                (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);
            
            double cr_y = 0.5 * ((2 * p1.Y) + 
                (-p0.Y + p2.Y) * t + 
                (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 + 
                (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

            // Linear interpolation
            double lin_x = p1.X + (p2.X - p1.X) * t;
            double lin_y = p1.Y + (p2.Y - p1.Y) * t;

            // Blend based on tension
            return new Point(
                lin_x * (1 - tension) + cr_x * tension,
                lin_y * (1 - tension) + cr_y * tension
            );
        }

        /// <summary>
        /// Creates PathGeometry from interpolated points
        /// </summary>
        private static PathGeometry CreatePathGeometry(Point startPoint, List<Point> points)
        {
            var polyLineSegment = new PolyLineSegment();
            foreach (var point in points)
            {
                polyLineSegment.Points.Add(point);
            }

            var pathFigure = new PathFigure
            {
                IsClosed = false,
                IsFilled = false,
                StartPoint = startPoint,
                Segments = new PathSegmentCollection { polyLineSegment }
            };

            return new PathGeometry 
            { 
                Figures = new PathFigureCollection { pathFigure } 
            };
        }

        #endregion
    }
}
