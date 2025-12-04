using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Linq;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.ViewModel;
using Windows.Foundation;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// Dashboard page displaying business metrics and charts
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        #region Fields

        private readonly DashboardViewModel _viewModel;
        private Ellipse? _hoverDot;
        private Border? _hoverTooltip;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DashboardPage
        /// </summary>
        public DashboardPage()
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel();
            DataContext = _viewModel;
            
            Loaded += DashboardPage_Loaded;
            SizeChanged += DashboardPage_SizeChanged;
            
            if (MonthlyChartBorder != null)
            {
                MonthlyChartBorder.SizeChanged += MonthlyChartBorder_SizeChanged;
            }
        }

        #endregion

        #region Event Handlers - Lifecycle

        /// <summary>
        /// Handles page loaded event
        /// </summary>
        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= DashboardPage_Loaded;
            await _viewModel.LoadAsync();

            EnqueueChartUpdate();
        }

        /// <summary>
        /// Handles page size changed event
        /// </summary>
        private void DashboardPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            EnqueueChartUpdate();
        }

        #endregion

        #region Event Handlers - Chart Sizing

        /// <summary>
        /// Handles desktop chart size changed event
        /// </summary>
        private void MonthlyChartBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsDesktopLayoutVisible())
            {
                UpdateDesktopChart();
            }
        }

        /// <summary>
        /// Handles tablet chart size changed event
        /// </summary>
        private void MonthlyChartBorderTablet_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsTabletLayoutVisible() && sender is Border border)
            {
                UpdateTabletChart(border);
            }
        }

        #endregion

        #region Event Handlers - Chart Interaction

        /// <summary>
        /// Handles pointer moved event for chart hover
        /// </summary>
        private void MonthlyChartBorder_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!CanShowChartHover()) return;

            var position = e.GetCurrentPoint(MonthlyChartBorder!).Position;
            var index = CalculateNearestDataPointIndex(position, MonthlyChartBorder.ActualWidth);
            
            if (index < 0) return;

            var (pointX, pointY) = CalculateChartPoint(index, MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
            
            EnsureHoverElements();
            UpdateHoverDot(pointX, pointY);
            UpdateHoverTooltip(index, pointX, pointY, MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
        }

        /// <summary>
        /// Handles pointer exited event for chart hover
        /// </summary>
        private void MonthlyChartBorder_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ClearHoverElements();
        }

        #endregion

        #region Chart Update Methods

        /// <summary>
        /// Enqueues chart geometry update on dispatcher
        /// </summary>
        private void EnqueueChartUpdate()
        {
            var dispatcher = DispatcherQueue.GetForCurrentThread();
            dispatcher?.TryEnqueue(UpdateChartGeometry);
        }

        /// <summary>
        /// Updates chart geometry based on current layout
        /// </summary>
        private void UpdateChartGeometry()
        {
            if (IsDesktopLayoutVisible())
            {
                UpdateDesktopChart();
            }
            else if (IsTabletLayoutVisible())
            {
                var tabletBorder = FindName("MonthlyChartBorderTablet") as Border;
                if (tabletBorder != null)
                {
                    UpdateTabletChart(tabletBorder);
                }
            }
        }

        /// <summary>
        /// Updates desktop chart and renders grid/axes
        /// </summary>
        private void UpdateDesktopChart()
        {
            if (MonthlyChartBorder == null || !HasValidDimensions(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight))
            {
                return;
            }

            _viewModel.UpdateMonthlyRevenueGeometry(MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
            
            if (_viewModel.MonthlyRevenue.Count > 0)
            {
                ChartRenderingHelper.RenderGrid(MonthlyGridCanvas, _viewModel.MonthlyRevenue, 
                    MonthlyChartBorder.ActualWidth, MonthlyChartBorder.ActualHeight);
                ChartRenderingHelper.RenderXAxis(MonthlyXAxisCanvas, _viewModel.MonthlyRevenue, 
                    MonthlyChartBorder.ActualWidth, isTablet: false);
            }
        }

        /// <summary>
        /// Updates tablet chart and renders grid/axes
        /// </summary>
        private void UpdateTabletChart(Border chartBorder)
        {
            if (!HasValidDimensions(chartBorder.ActualWidth, chartBorder.ActualHeight))
            {
                return;
            }

            _viewModel.UpdateTabletMonthlyRevenueGeometry(chartBorder.ActualWidth, chartBorder.ActualHeight);
            
            if (_viewModel.MonthlyRevenue.Count > 0)
            {
                var gridCanvas = FindName("MonthlyGridCanvasTablet") as Canvas;
                var xAxisCanvas = FindName("MonthlyXAxisCanvasTablet") as Canvas;

                if (gridCanvas != null)
                {
                    ChartRenderingHelper.RenderGrid(gridCanvas, _viewModel.MonthlyRevenue, 
                        chartBorder.ActualWidth, chartBorder.ActualHeight);
                }

                if (xAxisCanvas != null)
                {
                    ChartRenderingHelper.RenderXAxis(xAxisCanvas, _viewModel.MonthlyRevenue, 
                        chartBorder.ActualWidth, isTablet: true);
                }
            }
        }

        #endregion

        #region Hover Methods

        /// <summary>
        /// Checks if chart hover can be displayed
        /// </summary>
        private bool CanShowChartHover()
        {
            return _viewModel?.MonthlyRevenue != null && 
                   _viewModel.MonthlyRevenue.Count > 0 && 
                   MonthlyChartBorder != null && 
                   MonthlyHoverCanvas != null;
        }

        /// <summary>
        /// Calculates nearest data point index from pointer position
        /// </summary>
        private int CalculateNearestDataPointIndex(Point position, double containerWidth)
        {
            int n = _viewModel.MonthlyRevenue.Count;
            double w = Math.Max(ChartConfiguration.MinChartDimension, 
                containerWidth - ChartConfiguration.LeftPadding - ChartConfiguration.RightPadding);

            double xInterval = n == 1 ? 0 : w / (n - 1);
            int index = (int)Math.Round((position.X - ChartConfiguration.LeftPadding) / xInterval);
            
            return Math.Clamp(index, 0, n - 1);
        }

        /// <summary>
        /// Calculates chart point coordinates for given data index
        /// </summary>
        private (double x, double y) CalculateChartPoint(int index, double containerWidth, double containerHeight)
        {
            int n = _viewModel.MonthlyRevenue.Count;
            var values = _viewModel.MonthlyRevenue.Select(x => x.Revenue).ToList();
            
            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min;
            if (range == 0) range = 1;

            double w = Math.Max(ChartConfiguration.MinChartDimension, 
                containerWidth - ChartConfiguration.LeftPadding - ChartConfiguration.RightPadding);
            double h = Math.Max(ChartConfiguration.MinChartDimension, 
                containerHeight - ChartConfiguration.TopPadding - ChartConfiguration.BottomPadding);

            double x = ChartConfiguration.LeftPadding + (n == 1 ? w / 2 : (w * index) / (n - 1));
            double normalized = (double)((values[index] - min) / range);
            double y = ChartConfiguration.TopPadding + (1 - normalized) * h;

            return (x, y);
        }

        /// <summary>
        /// Ensures hover visual elements exist
        /// </summary>
        private void EnsureHoverElements()
        {
            if (MonthlyHoverCanvas == null) return;

            if (_hoverDot == null)
            {
                _hoverDot = CreateHoverDot();
                MonthlyHoverCanvas.Children.Add(_hoverDot);
            }

            if (_hoverTooltip == null)
            {
                _hoverTooltip = CreateHoverTooltip();
                MonthlyHoverCanvas.Children.Add(_hoverTooltip);
            }
        }

        /// <summary>
        /// Creates hover dot visual element
        /// </summary>
        private static Ellipse CreateHoverDot()
        {
            return new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = GetResourceBrush("TH.PrimaryBrush", Colors.CornflowerBlue),
                Stroke = GetResourceBrush("TH.CardBackground", Colors.White),
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
        }

        /// <summary>
        /// Creates hover tooltip visual element
        /// </summary>
        private static Border CreateHoverTooltip()
        {
            var textBlock = new TextBlock
            {
                Text = string.Empty,
                Foreground = GetResourceBrush("TH.TextPrimary", Colors.Black),
                FontSize = 12
            };

            return new Border
            {
                Background = GetResourceBrush("TH.CardBackground", Colors.White),
                BorderBrush = GetResourceBrush("TH.BorderBrush", Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6),
                Child = textBlock,
                IsHitTestVisible = false
            };
        }

        /// <summary>
        /// Updates hover dot position
        /// </summary>
        private void UpdateHoverDot(double x, double y)
        {
            if (_hoverDot == null) return;

            Canvas.SetLeft(_hoverDot, x - (_hoverDot.Width / 2));
            Canvas.SetTop(_hoverDot, y - (_hoverDot.Height / 2));
        }

        /// <summary>
        /// Updates hover tooltip position and content
        /// </summary>
        private void UpdateHoverTooltip(int dataIndex, double pointX, double pointY, double containerWidth, double containerHeight)
        {
            if (_hoverTooltip == null) return;

            // Update content
            var data = _viewModel.MonthlyRevenue[dataIndex];
            if (_hoverTooltip.Child is TextBlock textBlock)
            {
                textBlock.Text = $"{data.Date:dd/MM}: {data.Revenue:C0}";
            }

            // Calculate position
            _hoverTooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double tipWidth = _hoverTooltip.DesiredSize.Width;
            double tipHeight = _hoverTooltip.DesiredSize.Height;

            double tipX = CalculateTooltipX(pointX, tipWidth, containerWidth);
            double tipY = CalculateTooltipY(pointY, tipHeight, containerHeight);

            Canvas.SetLeft(_hoverTooltip, tipX);
            Canvas.SetTop(_hoverTooltip, tipY);
        }

        /// <summary>
        /// Calculates tooltip X position with boundary constraints
        /// </summary>
        private static double CalculateTooltipX(double pointX, double tooltipWidth, double containerWidth)
        {
            double tipX = pointX + 8;
            double maxX = containerWidth - ChartConfiguration.RightPadding - tooltipWidth;

            if (tipX > maxX)
            {
                tipX = pointX - 8 - tooltipWidth;
            }

            return Math.Max(ChartConfiguration.LeftPadding, tipX);
        }

        /// <summary>
        /// Calculates tooltip Y position with boundary constraints
        /// </summary>
        private static double CalculateTooltipY(double pointY, double tooltipHeight, double containerHeight)
        {
            double tipY = pointY - tooltipHeight - 8;
            double minY = ChartConfiguration.TopPadding;
            double maxY = containerHeight - ChartConfiguration.BottomPadding - tooltipHeight;

            if (tipY < minY)
            {
                tipY = pointY + 8;
            }

            return Math.Clamp(tipY, minY, maxY);
        }

        /// <summary>
        /// Clears hover visual elements
        /// </summary>
        private void ClearHoverElements()
        {
            MonthlyHoverCanvas?.Children.Clear();
            _hoverDot = null;
            _hoverTooltip = null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if desktop layout is currently visible
        /// </summary>
        private bool IsDesktopLayoutVisible()
        {
            return DesktopLayout?.Visibility == Visibility.Visible && MonthlyChartBorder != null;
        }

        /// <summary>
        /// Checks if tablet layout is currently visible
        /// </summary>
        private bool IsTabletLayoutVisible()
        {
            return TabletLayout?.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Validates chart container dimensions
        /// </summary>
        private static bool HasValidDimensions(double width, double height)
        {
            return width > 0 && height > 0 && 
                   !double.IsNaN(width) && !double.IsNaN(height);
        }

        /// <summary>
        /// Gets a brush resource from application with fallback color
        /// </summary>
        private static Brush GetResourceBrush(string resourceKey, Windows.UI.Color fallbackColor)
        {
            if (Application.Current.Resources.ContainsKey(resourceKey))
            {
                return (Brush)Application.Current.Resources[resourceKey];
            }
            return new SolidColorBrush(fallbackColor);
        }

        #endregion
    }
}
