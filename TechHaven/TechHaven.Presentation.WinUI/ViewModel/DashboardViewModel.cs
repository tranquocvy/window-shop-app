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
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Dashboard;
using Windows.Foundation;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        private static readonly HttpClient SharedHttpClient = ApiClientFactory.GetHttpClient();
        private readonly IDashboardService _service;

        public DashboardViewModel() : this(new HttpDashboardService(SharedHttpClient)) { }
        //public DashboardViewModel() : this(new MockDashboardService()) { }

        public DashboardViewModel(IDashboardService service)
        {
            _service = service;
            LowStockProducts = new ObservableCollection<LowStockProductDto>();
            TopSellingProducts = new ObservableCollection<TopSellingProductDto>();
            RecentOrders = new ObservableCollection<RecentOrderDto>();
            MonthlyRevenue = new ObservableCollection<DailyRevenueDto>();
        }

        [ObservableProperty]
        private int totalProducts;

        [ObservableProperty]
        private int totalOrders;

        [ObservableProperty]
        private int todayOrderCount;

        [ObservableProperty]
        private string todayRevenue = string.Empty;

        [ObservableProperty]
        private Geometry monthlyRevenueGeometry;

        // Spline configuration: tension (0..1) and samples per segment (smoothness)
        [ObservableProperty]
        private double splineTension = 1.0; // 0 = linear, 1 = full Catmull-Rom

        [ObservableProperty]
        private int splineSamplesPerSegment = 12; // number of interpolation steps per segment

        public ObservableCollection<LowStockProductDto> LowStockProducts { get; }

        public ObservableCollection<TopSellingProductDto> TopSellingProducts { get; }

        public ObservableCollection<RecentOrderDto> RecentOrders { get; }

        public ObservableCollection<DailyRevenueDto> MonthlyRevenue { get; }

        public async Task LoadAsync()
        {
            var resp = await _service.GetDashboardAsync();
            if (resp?.Success == true && resp.Data != null)
            {
                var d = resp.Data;

                TotalProducts = d.TotalProducts;
                TodayOrderCount = d.TodayOrderCount;
                TodayRevenue = d.TodayRevenue.ToString("C");

                LowStockProducts.Clear();
                foreach (var item in d.LowStockProducts ?? new()) LowStockProducts.Add(item);

                TopSellingProducts.Clear();
                foreach (var item in d.TopSellingProducts ?? new()) TopSellingProducts.Add(item);

                RecentOrders.Clear();
                if (d.RecentOrders != null)
                {
                    foreach (var item in d.RecentOrders)
                    {
                        RecentOrders.Add(item);
                    }
                }

                MonthlyRevenue.Clear();
                if (d.MonthlyRevenue != null)
                {
                    foreach (var item in d.MonthlyRevenue)
                        MonthlyRevenue.Add(item);
                }
            }
        }

        // Call this from UI when chart container size changes
        public void UpdateMonthlyRevenueGeometry(double width, double height)
        {
            if (MonthlyRevenue == null || MonthlyRevenue.Count == 0)
            {
                MonthlyRevenueGeometry = null;
                return;
            }

            var values = MonthlyRevenue.Select(x => x.Revenue).ToList();
            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min;
            if (range == 0) range = 1; // avoid divide by zero

            int n = values.Count;
            // leave some padding 
            double leftPadding = 62;
            double topPadding = 12;
            double rightPadding = 18;
            double bottomPadding = 36;

            double w = Math.Max(10, width - leftPadding - rightPadding);
            double h = Math.Max(10, height - topPadding - bottomPadding);

            // compute points
            var points = new List<Point>(n);
            for (int i = 0; i < n; i++)
            {
                double x = leftPadding + (n == 1 ? w / 2 : (w * i) / (n - 1));
                double normalized = (double)((values[i] - min) / range); // 0..1
                double y = topPadding + (1 - normalized) * h; // invert y
                points.Add(new Point(x, y));
            }

            // clamp samples
            int samples = Math.Max(1, splineSamplesPerSegment);
            double tension = Math.Clamp(splineTension, 0.0, 1.0);

            // build PathFigure using Catmull-Rom sampling between points
            var fig = new PathFigure
            {
                IsClosed = false,
                IsFilled = false,
                StartPoint = points[0]
            };

            var allInterpPoints = new List<Point>();

            if (n == 1)
            {
                // single point: nothing to draw
                MonthlyRevenueGeometry = null;
                return;
            }
            else
            {
                // iterate segments between points[i] (P1) and points[i+1] (P2)
                for (int i = 0; i < n - 1; i++)
                {
                    Point p0 = (i - 1) >= 0 ? points[i - 1] : points[i];
                    Point p1 = points[i];
                    Point p2 = points[i + 1];
                    Point p3 = (i + 2) < n ? points[i + 2] : points[i + 1];

                    // generate samples for this segment (exclude t=0 because it's previous point)
                    for (int s = 1; s <= samples; s++)
                    {
                        double t = (double)s / samples;

                        // Catmull-Rom standard basis (with 0.5 tension factor)
                        // CR point:
                        double t2 = t * t;
                        double t3 = t2 * t;

                        double cr_x = 0.5 * ((2 * p1.X) + (-p0.X + p2.X) * t + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);
                        double cr_y = 0.5 * ((2 * p1.Y) + (-p0.Y + p2.Y) * t + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

                        // linear interpolation for blending
                        double lin_x = p1.X + (p2.X - p1.X) * t;
                        double lin_y = p1.Y + (p2.Y - p1.Y) * t;

                        // blend between linear (0) and CR (1) according to tension property
                        double blended_x = lin_x * (1 - tension) + cr_x * tension;
                        double blended_y = lin_y * (1 - tension) + cr_y * tension;

                        allInterpPoints.Add(new Point(blended_x, blended_y));
                    }
                }
            }

            // create PolyLineSegment with the interpolated points
            var poly = new PolyLineSegment();
            foreach (var p in allInterpPoints)
            {
                poly.Points.Add(p);
            }

            var segs = new PathSegmentCollection { poly };
            fig.Segments = segs;

            var figs = new PathFigureCollection { fig };
            var pathGeom = new PathGeometry { Figures = figs };
            MonthlyRevenueGeometry = pathGeom;
        }
    }
}
