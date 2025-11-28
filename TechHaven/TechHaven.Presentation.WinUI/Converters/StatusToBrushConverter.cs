using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (value as string)?.ToLowerInvariant() ?? string.Empty;
            switch (status)
            {
                case "completed":
                case "done":
                    return new SolidColorBrush(Microsoft.UI.Colors.Green);
                case "processing":
                case "inprogress":
                    return new SolidColorBrush(Microsoft.UI.Colors.Orange);
                case "cancelled":
                case "canceled":
                    return new SolidColorBrush(Microsoft.UI.Colors.Red);
                case "pending":
                    return new SolidColorBrush(Microsoft.UI.Colors.Gray);
                default:
                    return new SolidColorBrush(Microsoft.UI.Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
