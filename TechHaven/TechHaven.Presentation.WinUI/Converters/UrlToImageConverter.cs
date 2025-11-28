using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Converters
{
    public class UrlToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var url = value as string;
            if (string.IsNullOrWhiteSpace(url))
            {
                // fallback: trả về null hoặc ảnh mặc định trong Assets
                return new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
            }

            try
            {
                // Tạo BitmapImage từ Uri (hỗ trợ http/https)
                return new BitmapImage(new Uri(url));
            }
            catch
            {
                // nếu URL không hợp lệ hoặc load lỗi, trả về ảnh mặc định
                return new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
