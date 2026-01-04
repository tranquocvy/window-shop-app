using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (window == null) throw new ArgumentNullException(nameof(window));

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

        /// <summary>
        /// Hiển thị dialog thông báo lỗi với danh sách errors
        /// </summary>
        public static async Task ShowErrorAsync(
            Window window,
            string title,
            string message,
            IEnumerable<string>? errors = null)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            var content = message;
            if (errors != null && errors.Any())
            {
                content += "\n\nChi tiết lỗi:\n" + string.Join("\n", errors.Select(e => "• " + e));
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Hiển thị dialog thông báo thành công
        /// </summary>
        public static async Task ShowSuccessAsync(
            Window window,
            string title,
            string message)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Hiển thị dialog cảnh báo
        /// </summary>
        public static async Task ShowWarningAsync(
            Window window,
            string title,
            string message)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Đóng",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
