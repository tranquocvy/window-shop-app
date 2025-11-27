using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Dashboard;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Linq;
using System;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IDashboardService _service;

        public DashboardViewModel() : this(new MockDashboardService()) { }

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
            double leftPadding = 8;
            double topPadding = 8;
            double rightPadding = 8;
            double bottomPadding = 16;

            double w = Math.Max(10, width - leftPadding - rightPadding);
            double h = Math.Max(10, height - topPadding - bottomPadding);

            // compute points
            var points = new System.Collections.Generic.List<System.Numerics.Vector2>(n);
            for (int i = 0; i < n; i++)
            {
                double x = leftPadding + (n == 1 ? w / 2 : (w * i) / (n - 1));
                double normalized = (double)((values[i] - min) / range); // 0..1
                double y = topPadding + (1 - normalized) * h; // invert y
                points.Add(new System.Numerics.Vector2((float)x, (float)y));
            }

            // build PathFigure with Bezier smoothing
            var fig = new PathFigure();
            fig.IsClosed = false;
            fig.IsFilled = false;
            fig.StartPoint = new Windows.Foundation.Point(points[0].X, points[0].Y);

            var segs = new PathSegmentCollection();

            for (int i = 1; i < points.Count; i++)
            {
                var p0 = points[i - 1];
                var p1 = points[i];

                // control points at midpoint x with p0.y and p1.y
                double cx = (p0.X + p1.X) / 2;
                var c1 = new BezierSegment();
                c1.Point1 = new Windows.Foundation.Point(cx, p0.Y);
                c1.Point2 = new Windows.Foundation.Point(cx, p1.Y);
                c1.Point3 = new Windows.Foundation.Point(p1.X, p1.Y);
                segs.Add(c1);
            }

            fig.Segments = segs;

            var figs = new PathFigureCollection { fig };
            var pathGeom = new PathGeometry { Figures = figs };
            MonthlyRevenueGeometry = pathGeom;
        }
    }
}
