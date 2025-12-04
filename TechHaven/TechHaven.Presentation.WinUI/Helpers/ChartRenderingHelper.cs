using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using TechHaven.Shared.DTOs.Dashboard;
using Windows.Foundation;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Helper class for rendering chart grid and axes
    /// </summary>
    public static class ChartRenderingHelper
    {
        /// <summary>
        /// Renders horizontal grid lines and Y-axis labels
        /// </summary>
        /// <param name="canvas">Canvas to render on</param>
        /// <param name="data">Revenue data</param>
        /// <param name="containerWidth">Container width</param>
        /// <param name="containerHeight">Container height</param>
        public static void RenderGrid(Canvas canvas, IReadOnlyList<DailyRevenueDto> data, double containerWidth, double containerHeight)
        {
            if (canvas == null || data == null || data.Count == 0) return;
            if (containerWidth <= 0 || containerHeight <= 0) return;

            var values = data.Select(x => x.Revenue).ToList();
            decimal min = values.Min();
            decimal max = values.Max();
            decimal range = max - min;
            if (range == 0) range = 1;

            double w = Math.Max(ChartConfiguration.MinChartDimension, 
                containerWidth - ChartConfiguration.LeftPadding - ChartConfiguration.RightPadding);
            double h = Math.Max(ChartConfiguration.MinChartDimension, 
                containerHeight - ChartConfiguration.TopPadding - ChartConfiguration.BottomPadding);

            canvas.Children.Clear();

            var gridBrush = GetResourceBrush("TH.BorderBrush", Colors.LightGray);
            var labelBrush = GetResourceBrush("TH.TextDisabled", Colors.Gray);

            // Render horizontal grid lines with labels
            for (int i = 1; i <= ChartConfiguration.GridLineCount; i++)
            {
                double fraction = i / (double)(ChartConfiguration.GridLineCount + 1);
                double y = ChartConfiguration.TopPadding + (1 - fraction) * h;

                // Grid line
                var line = new Line
                {
                    X1 = ChartConfiguration.LeftPadding,
                    X2 = ChartConfiguration.LeftPadding + w,
                    Y1 = y,
                    Y2 = y,
                    Stroke = gridBrush,
                    StrokeThickness = 1,
                    Opacity = ChartConfiguration.GridLineOpacity
                };
                canvas.Children.Add(line);

                // Y-axis label
                decimal valueAt = min + (decimal)fraction * range;
                var label = new TextBlock
                {
                    Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:C0}", valueAt),
                    Foreground = labelBrush,
                    FontSize = ChartConfiguration.DesktopLabelFontSize
                };
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(label, 2);
                Canvas.SetTop(label, y - label.DesiredSize.Height / 2);
                canvas.Children.Add(label);
            }
        }

        /// <summary>
        /// Renders X-axis ticks and date labels
        /// </summary>
        /// <param name="canvas">Canvas to render on</param>
        /// <param name="data">Revenue data</param>
        /// <param name="containerWidth">Container width</param>
        /// <param name="isTablet">Whether this is for tablet layout</param>
        public static void RenderXAxis(Canvas canvas, IReadOnlyList<DailyRevenueDto> data, double containerWidth, bool isTablet = false)
        {
            if (canvas == null || data == null || data.Count == 0) return;
            if (containerWidth <= 0) return;

            int n = data.Count;
            double w = Math.Max(ChartConfiguration.MinChartDimension, 
                containerWidth - ChartConfiguration.LeftPadding - ChartConfiguration.RightPadding);

            canvas.Children.Clear();

            var tickBrush = GetResourceBrush("TH.BorderBrush", Colors.Gray);
            var labelBrush = GetResourceBrush("TH.TextDisabled", Colors.Gray);
            var fontSize = isTablet ? ChartConfiguration.TabletLabelFontSize : ChartConfiguration.DesktopLabelFontSize;
            var maxPointsForAll = isTablet ? ChartConfiguration.TabletMaxPointsForAllLabels : ChartConfiguration.MaxPointsForAllLabels;

            for (int i = 0; i < n; i++)
            {
                double x = ChartConfiguration.LeftPadding + (n == 1 ? w / 2 : (w * i) / (n - 1));

                // Render tick
                var tick = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = ChartConfiguration.TickTop,
                    Y2 = ChartConfiguration.TickBottom,
                    Stroke = tickBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // Render label (with spacing logic)
                bool shouldShowLabel = n <= maxPointsForAll || 
                                      i % ChartConfiguration.XAxisLabelInterval == 0 || 
                                      i == n - 1 || 
                                      i == 0;

                if (shouldShowLabel)
                {
                    var date = data[i].Date;
                    var label = new TextBlock
                    {
                        Text = date.ToString("dd/MM"),
                        Foreground = labelBrush,
                        FontSize = fontSize
                    };
                    label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(label, x - label.DesiredSize.Width / 2);
                    Canvas.SetTop(label, ChartConfiguration.LabelTop);
                    canvas.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Gets a brush resource from application resources with fallback
        /// </summary>
        private static Brush GetResourceBrush(string resourceKey, Windows.UI.Color fallbackColor)
        {
            if (Application.Current.Resources.ContainsKey(resourceKey))
            {
                return (Brush)Application.Current.Resources[resourceKey];
            }
            return new SolidColorBrush(fallbackColor);
        }
    }
}
