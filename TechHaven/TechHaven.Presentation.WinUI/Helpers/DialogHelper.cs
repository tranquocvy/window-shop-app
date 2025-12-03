using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class DialogHelper
    {
        /// <summary>
        /// Hiển thị dialog xác nhận với Window hiện tại.
        /// Trả về true nếu người dùng bấm PrimaryButton (ví dụ "Xóa"), false nếu bấm Hủy.
        /// </summary>
        /// <param name="window">Window hiện tại</param>
        /// <param name="title">Tiêu đề dialog</param>
        /// <param name="content">Nội dung dialog</param>
        /// <param name="primaryButton">Tên nút chính (mặc định "Xóa")</param>
        /// <param name="closeButton">Tên nút đóng/hủy (mặc định "Hủy")</param>
        public static async Task<bool> ShowConfirmAsync(
            Window window,
            string title,
            string content,
            string primaryButton = "Xóa",
            string closeButton = "Hủy")
        {
            if (window == null) throw new System.ArgumentNullException(nameof(window));

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButton,
                CloseButtonText = closeButton,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = window.Content.XamlRoot 
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
