using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class TabButtonBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var selectedTab = value as string;
            var tabName = parameter as string;

            if (selectedTab == tabName)
            {
                // Active tab - use primary brush
                return Application.Current.Resources["TH.PrimaryBrush"] as SolidColorBrush;
            }
            else
            {
                // Inactive tab - use card background
                return Application.Current.Resources["TH.CardBackground"] as SolidColorBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
