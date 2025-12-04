using System;
using System.Globalization;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class PriceFormatter
    {
        /// <summary>
        /// Định dạng số mặc định (có thể thay bằng format khác).
        /// Ví dụ: "#,0", "0,0.00", "###,###"
        /// </summary>
        public static string NumberFormat = "#,0";

        /// <summary>
        /// Quốc gia mặc định (có thể chỉnh thành "en-US", "ja-JP", "ko-KR", ...)
        /// </summary>
        public static string DefaultCulture = "vi-VN";

        /// <summary>
        /// Format theo quốc gia mặc định
        /// </summary>
        public static string Format(decimal value)
            => Format(value, DefaultCulture);

        public static string Format(double value)
            => Format(value, DefaultCulture);

        public static string Format(long value)
            => Format(value, DefaultCulture);

        /// <summary>
        /// Format theo quốc gia cụ thể
        /// </summary>
        public static string Format(decimal value, string culture)
        {
            return value.ToString(NumberFormat, new CultureInfo(culture));
        }

        public static string Format(double value, string culture)
        {
            return value.ToString(NumberFormat, new CultureInfo(culture));
        }

        public static string Format(long value, string culture)
        {
            return value.ToString(NumberFormat, new CultureInfo(culture));
        }
    }
}
