using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using TechHaven.Shared.DTOs.Customers;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class CustomerTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CustomerType type)
            {
                return type switch
                {
                    CustomerType.Regular => new SolidColorBrush(Colors.DodgerBlue),
                    CustomerType.Student => new SolidColorBrush(Colors.Green),
                    CustomerType.VIP => new SolidColorBrush(Colors.Gold),
                    _ => new SolidColorBrush(Colors.Gray),
                };
            }

            // If value is nullable or string, try parse
            if (value != null && Enum.TryParse(typeof(CustomerType), value.ToString(), out var parsed))
            {
                var t = (CustomerType)parsed;
                return t switch
                {
                    CustomerType.Regular => new SolidColorBrush(Colors.DodgerBlue),
                    CustomerType.Student => new SolidColorBrush(Colors.Green),
                    CustomerType.VIP => new SolidColorBrush(Colors.Gold),
                    _ => new SolidColorBrush(Colors.Gray),
                };
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
