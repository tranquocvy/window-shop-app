using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TechHaven.Presentation.WinUI.ViewModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using System.Linq;

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
                RenderMonthlyXAxis();
            });
        }

        private void MonthlyChartBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _vm.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
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

            double leftPadding = 8;
            double rightPadding = 8;

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

        private void MonthlyChartBorder_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_vm == null || _vm.MonthlyRevenue == null || _vm.MonthlyRevenue.Count == 0) return;

            var pos = e.GetCurrentPoint(MonthlyChartBorder).Position;

            double borderWidth = MonthlyChartBorder.ActualWidth;
            double leftPadding = 8;
            double rightPadding = 8;
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
            double topPadding = 8; double bottomPadding = 16; double h = Math.Max(10, MonthlyChartBorder.ActualHeight - topPadding - bottomPadding);

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

                double tipX = px + 8;
                double tipY = Math.Max(0, py - 24);
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
