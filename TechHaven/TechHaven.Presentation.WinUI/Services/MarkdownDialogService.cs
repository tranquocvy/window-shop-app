using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using System;

namespace TechHaven.Presentation.WinUI.Services
{
    /// <summary>
    /// Service to display AI responses with markdown formatting
    /// </summary>
    public static class MarkdownDialogService
    {
        /// <summary>
        /// Show AI response in a formatted dialog with markdown support
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="markdownContent">Markdown formatted content from AI</param>
        /// <param name="xamlRoot">XamlRoot for the dialog</param>
        /// <returns>Dialog result</returns>
        public static async Task<ContentDialogResult> ShowMarkdownDialogAsync(
            string title, 
            string markdownContent, 
            Microsoft.UI.Xaml.XamlRoot xamlRoot)
        {
            var parsedContent = MarkdownHelper.ParseMarkdown(markdownContent);

            var scrollViewer = new ScrollViewer
            {
                Content = parsedContent,
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = scrollViewer,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            return await dialog.ShowAsync().AsTask();
        }

        /// <summary>
        /// Show AI response with action buttons
        /// </summary>
        public static async Task<ContentDialogResult> ShowMarkdownDialogWithActionsAsync(
            string title,
            string markdownContent,
            string primaryButtonText,
            string secondaryButtonText,
            Microsoft.UI.Xaml.XamlRoot xamlRoot)
        {
            var parsedContent = MarkdownHelper.ParseMarkdown(markdownContent);

            var scrollViewer = new ScrollViewer
            {
                Content = parsedContent,
                MaxHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = scrollViewer,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            return await dialog.ShowAsync().AsTask();
        }

        /// <summary>
        /// Create a formatted StackPanel from markdown (for embedding in existing UI)
        /// </summary>
        public static StackPanel CreateMarkdownPanel(string markdownContent)
        {
            return MarkdownHelper.ParseMarkdown(markdownContent);
        }
    }
}
