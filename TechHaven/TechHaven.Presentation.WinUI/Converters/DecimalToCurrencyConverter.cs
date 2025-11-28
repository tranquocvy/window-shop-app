using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class DecimalToCurrencyConverter : IValueConverter
    {
        // Converts numeric value (decimal/double/int) to formatted currency string for Vietnamese Dong
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If null -> show 0 ₫ per request
            if (value == null) return "0 VNĐ";

            try
            {
                decimal amount;

                switch (value)
                {
                    case decimal d:
                        amount = d;
                        break;
                    case double db:
                        amount = (decimal)db;
                        break;
                    case float f:
                        amount = (decimal)f;
                        break;
                    case int i:
                        amount = i;
                        break;
                    case long l:
                        amount = l;
                        break;
                    case string s when decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed):
                        amount = parsed;
                        break;
                    default:
                        // If we cannot parse, return original ToString or 0
                        return "0 VNĐ";
                }

                // If zero, show explicit 0 ₫
                if (amount == 0m) return "0 VNĐ";

                // Format using vi-VN culture, no decimals for VND, group separators
                var culture = new CultureInfo("vi-VN");
                var formatted = string.Format(culture, "{0:N0}", amount);

                // Append currency sign (₫)
                return formatted + " VNĐ";
            }
            catch
            {
                return "0 VNĐ";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
