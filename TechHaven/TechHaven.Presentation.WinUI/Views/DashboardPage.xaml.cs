using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Linq;
using TechHaven.Presentation.WinUI.ViewModel;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    public sealed partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _vm = new();

        private Ellipse? _hoverDot;
        private Border? _hoverTooltip;

        public DashboardPage()
        {
            InitializeComponent();
            this.DataContext = _vm;
            this.Loaded += DashboardPage_Loaded;
            MonthlyChartBorder.SizeChanged += MonthlyChartBorder_SizeChanged;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= DashboardPage_Loaded;
            await _vm.LoadAsync();

            // Enqueue geometry update after layout pass to ensure ActualWidth/Height are valid
            var dq = DispatcherQueue.GetForCurrentThread();
            dq?.TryEnqueue(() =>
            {
                _vm.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
                RenderMonthlyGrid();
                RenderMonthlyXAxis();
            });
        }

        private void MonthlyChartBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _vm.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
            RenderMonthlyGrid();
            RenderMonthlyXAxis();
        }
        private void RenderMonthlyXAxis()
        {
            if (_vm == null || _vm.MonthlyRevenue == null) return;

            var data = _vm.MonthlyRevenue;
            int n = data.Count;
            if (n == 0) return;

            double borderWidth = MonthlyChartBorder.ActualWidth;
            if (double.IsNaN(borderWidth) || borderWidth <= 0) borderWidth = MonthlyChartBorder.RenderSize.Width;

            double leftPadding = 62;
            double rightPadding = 18;

            double w = Math.Max(10, borderWidth - leftPadding - rightPadding);

            MonthlyXAxisCanvas.Children.Clear();

            Brush tickBrush = Application.Current.Resources.ContainsKey("TH.BorderBrush") ? (Brush)Application.Current.Resources["TH.BorderBrush"] : new SolidColorBrush(Colors.Gray);
            Brush labelBrush = Application.Current.Resources.ContainsKey("TH.TextDisabled") ? (Brush)Application.Current.Resources["TH.TextDisabled"] : new SolidColorBrush(Colors.Gray);

            double tickTop = 2;
            double tickBottom = 10;
            double labelTop = 12;

            for (int i = 0; i < n; i++)
            {
                double x = leftPadding + (n == 1 ? w / 2 : (w * i) / (n - 1));

                // vertical tick
                var tick = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = tickTop,
                    Y2 = tickBottom,
                    Stroke = tickBrush,
                    StrokeThickness = 1
                };
                MonthlyXAxisCanvas.Children.Add(tick);

                if (n <= 12 || i % 5 == 0 || i == n - 1 || i == 0)
                {
                    var date = data[i].Date;
                    var txt = new TextBlock
                    {
                        Text = date.ToString("dd/MM"),
                        Foreground = labelBrush,
                        FontSize = 11
                    };
                    txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    var lw = txt.DesiredSize.Width;
                    Canvas.SetLeft(txt, x - lw / 2);
                    Canvas.SetTop(txt, labelTop);
                    MonthlyXAxisCanvas.Children.Add(txt);
                }
            }
        }

        private void RenderMonthlyGrid()
        {
            if (_vm == null || _vm.MonthlyRevenue == null) return;

            var values = _vm.MonthlyRevenue.Select(x => x.Revenue).ToList();
            if (values.Count == 0) return;

            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min; if (range == 0) range = 1;

            double borderWidth = MonthlyChartBorder.ActualWidth;
            double borderHeight = MonthlyChartBorder.ActualHeight;
            if (double.IsNaN(borderWidth) || borderWidth <= 0) borderWidth = MonthlyChartBorder.RenderSize.Width;
            if (double.IsNaN(borderHeight) || borderHeight <= 0) borderHeight = MonthlyChartBorder.RenderSize.Height;

            double leftPadding = 62; double topPadding = 12; double rightPadding = 18; double bottomPadding = 36;
            double w = Math.Max(10, borderWidth - leftPadding - rightPadding);
            double h = Math.Max(10, borderHeight - topPadding - bottomPadding);

            MonthlyGridCanvas.Children.Clear();

            Brush gridBrush = Application.Current.Resources.ContainsKey("TH.BorderBrush") ? (Brush)Application.Current.Resources["TH.BorderBrush"] : new SolidColorBrush(Colors.LightGray);
            Brush labelBrush = Application.Current.Resources.ContainsKey("TH.TextDisabled") ? (Brush)Application.Current.Resources["TH.TextDisabled"] : new SolidColorBrush(Colors.Gray);

            // 3 horizontal lines at 25%,50%,75% of value range
            for (int i = 1; i <= 3; i++)
            {
                double frac = i / 4.0; // 0.25,0.5,0.75
                double y = topPadding + (1 - frac) * h;

                var line = new Line
                {
                    X1 = leftPadding,
                    X2 = leftPadding + w,
                    Y1 = y,
                    Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    Opacity = 0.6
                };
                MonthlyGridCanvas.Children.Add(line);

                // label value on left side
                decimal valueAt = min + (decimal)frac * range;
                var txt = new TextBlock
                {
                    Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C0}", valueAt),
                    Foreground = labelBrush,
                    FontSize = 11
                };
                txt.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(txt, 2);
                Canvas.SetTop(txt, y - txt.DesiredSize.Height / 2);
                MonthlyGridCanvas.Children.Add(txt);
            }
        }

        private void MonthlyChartBorder_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_vm == null || _vm.MonthlyRevenue == null || _vm.MonthlyRevenue.Count == 0) return;

            var pos = e.GetCurrentPoint(MonthlyChartBorder).Position;

            double borderWidth = MonthlyChartBorder.ActualWidth;
            double leftPadding = 62;
            double rightPadding = 18;
            double w = Math.Max(10, borderWidth - leftPadding - rightPadding);

            int n = _vm.MonthlyRevenue.Count;
            // compute nearest index by mapping pos.X to index
            double xPer = (n == 1) ? 0 : w / (n - 1);
            int idx = (int)Math.Round((pos.X - leftPadding) / (xPer));
            idx = Math.Max(0, Math.Min(n - 1, idx));

            // compute point coords
            var values = _vm.MonthlyRevenue.Select(x => x.Revenue).ToList();
            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min; if (range == 0) range = 1;
            double topPadding = 12; double bottomPadding = 36; double h = Math.Max(10, MonthlyChartBorder.ActualHeight - topPadding - bottomPadding);

            double px = leftPadding + (n == 1 ? w / 2 : (w * idx) / (n - 1));
            double normalized = (double)((values[idx] - min) / range);
            double py = topPadding + (1 - normalized) * h;

            EnsureHoverElements();

            if (_hoverDot != null)
            {
                Canvas.SetLeft(_hoverDot, px - (_hoverDot.Width / 2));
                Canvas.SetTop(_hoverDot, py - (_hoverDot.Height / 2));
            }

            if (_hoverTooltip != null)
            {
                var date = _vm.MonthlyRevenue[idx].Date;
                var rev = _vm.MonthlyRevenue[idx].Revenue;
                var tb = _hoverTooltip.Child as TextBlock;
                if (tb != null) tb.Text = $"{date:dd/MM}: {rev:C0}";

                _hoverTooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double tipW = _hoverTooltip.DesiredSize.Width;
                double tipH = _hoverTooltip.DesiredSize.Height;

                double tipX = px + 8;
                double maxX = MonthlyChartBorder.ActualWidth - rightPadding - tipW;
                if (tipX > maxX)
                {
                    tipX = px - 8 - tipW;
                }
                if (tipX < leftPadding) tipX = leftPadding;

                double tipY = py - tipH - 8;
                double minY = topPadding;
                double maxY = MonthlyChartBorder.ActualHeight - bottomPadding - tipH;
                if (tipY < minY)
                {
                    // try below the point
                    tipY = py + 8;
                }
                if (tipY < minY) tipY = minY;
                if (tipY > maxY) tipY = maxY;

                Canvas.SetLeft(_hoverTooltip, tipX);
                Canvas.SetTop(_hoverTooltip, tipY);
            }
        }

        private void MonthlyChartBorder_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MonthlyHoverCanvas.Children.Clear();
            _hoverDot = null; _hoverTooltip = null;
        }

        private void EnsureHoverElements()
        {
            if (_hoverDot == null)
            {
                _hoverDot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Application.Current.Resources.ContainsKey("TH.PrimaryBrush") ? (Brush)Application.Current.Resources["TH.PrimaryBrush"] : new SolidColorBrush(Colors.CornflowerBlue),
                    Stroke = Application.Current.Resources.ContainsKey("TH.CardBackground") ? (Brush)Application.Current.Resources["TH.CardBackground"] : new SolidColorBrush(Colors.White),
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };
                MonthlyHoverCanvas.Children.Add(_hoverDot);
            }

            if (_hoverTooltip == null)
            {
                var txt = new TextBlock { Text = "", Foreground = Application.Current.Resources.ContainsKey("TH.TextPrimary") ? (Brush)Application.Current.Resources["TH.TextPrimary"] : new SolidColorBrush(Colors.Black), FontSize = 12 };
                _hoverTooltip = new Border
                {
                    Background = Application.Current.Resources.ContainsKey("TH.CardBackground") ? (Brush)Application.Current.Resources["TH.CardBackground"] : new SolidColorBrush(Colors.White),
                    BorderBrush = Application.Current.Resources.ContainsKey("TH.BorderBrush") ? (Brush)Application.Current.Resources["TH.BorderBrush"] : new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(6),
                    Child = txt,
                    IsHitTestVisible = false
                };
                MonthlyHoverCanvas.Children.Add(_hoverTooltip);
            }
        }
    }
}
