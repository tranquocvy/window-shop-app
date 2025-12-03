using System;
using Microsoft.UI.Xaml.Data;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return string.Empty;
            if (value is DateTime dt)
            {
                var format = parameter as string ?? "g"; // general date/time
                try
                {
                    return dt.ToString(format);
                }
                catch
                {
                    return dt.ToString();
                }
            }
            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // not required for one-way bindings
            if (value is string s && DateTime.TryParse(s, out var dt)) return dt;
            return value;
        }
    }
}
