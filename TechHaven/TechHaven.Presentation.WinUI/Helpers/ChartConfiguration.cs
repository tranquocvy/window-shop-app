namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Configuration constants for chart rendering
    /// </summary>
    public static class ChartConfiguration
    {
        /// <summary>
        /// Left padding for chart area in pixels
        /// </summary>
        public const double LeftPadding = 62;

        /// <summary>
        /// Top padding for chart area in pixels
        /// </summary>
        public const double TopPadding = 12;

        /// <summary>
        /// Right padding for chart area in pixels
        /// </summary>
        public const double RightPadding = 18;

        /// <summary>
        /// Bottom padding for chart area in pixels
        /// </summary>
        public const double BottomPadding = 36;

        /// <summary>
        /// Height of X-axis canvas in pixels
        /// </summary>
        public const double XAxisCanvasHeight = 36;

        /// <summary>
        /// Number of horizontal grid lines to display
        /// </summary>
        public const int GridLineCount = 3;

        /// <summary>
        /// Top position for axis ticks
        /// </summary>
        public const double TickTop = 2;

        /// <summary>
        /// Bottom position for axis ticks
        /// </summary>
        public const double TickBottom = 10;

        /// <summary>
        /// Top position for axis labels
        /// </summary>
        public const double LabelTop = 12;

        /// <summary>
        /// Font size for desktop axis labels
        /// </summary>
        public const double DesktopLabelFontSize = 11;

        /// <summary>
        /// Font size for tablet axis labels
        /// </summary>
        public const double TabletLabelFontSize = 10;

        /// <summary>
        /// Interval for showing X-axis labels (every nth point)
        /// </summary>
        public const int XAxisLabelInterval = 5;

        /// <summary>
        /// Maximum number of points to show all X-axis labels
        /// </summary>
        public const int MaxPointsForAllLabels = 12;

        /// <summary>
        /// Maximum number of points for tablet to show all labels
        /// </summary>
        public const int TabletMaxPointsForAllLabels = 7;

        /// <summary>
        /// Grid line opacity
        /// </summary>
        public const double GridLineOpacity = 0.6;

        /// <summary>
        /// Minimum chart dimension
        /// </summary>
        public const double MinChartDimension = 10;
    }
}
