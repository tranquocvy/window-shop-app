using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class PriceFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            if (value is decimal dec)
                return Helpers.PriceFormatter.Format(dec);

            if (value is double dbl)
                return Helpers.PriceFormatter.Format(dbl);

            if (value is long lng)
                return Helpers.PriceFormatter.Format(lng);

            if (value is int i)
                return Helpers.PriceFormatter.Format(i);

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Không cần convert back
            return value;
        }
    }
}
