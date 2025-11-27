using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return string.Empty;

            if (value is decimal dec) return dec.ToString("C0", CultureInfo.CurrentCulture);
            if (value is double d) return d.ToString("C0", CultureInfo.CurrentCulture);
            if (value is float f) return f.ToString("C0", CultureInfo.CurrentCulture);
            if (value is int i) return i.ToString("C0", CultureInfo.CurrentCulture);
            if (value is long l) return l.ToString("C0", CultureInfo.CurrentCulture);

            // fallback
            if (decimal.TryParse(value.ToString(), out var parsed))
                return parsed.ToString("C0", CultureInfo.CurrentCulture);

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // not needed for one-way bindings
            return value;
        }
    }
}
