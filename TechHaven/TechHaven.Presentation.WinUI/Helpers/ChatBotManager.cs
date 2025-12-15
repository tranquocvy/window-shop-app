using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Manages chatbot UI and interactions
    /// </summary>
    public class ChatBotManager
    {
        private readonly Button _minimizedButton;
        private readonly Grid _expandedWindow;
        private readonly TextBox _messageInputBox;
        private readonly Button _sendButton;
        private readonly StackPanel _messagesPanel;
        private readonly ScrollViewer _messageScrollViewer;

        public ChatBotManager(
            Button minimizedButton,
            Grid expandedWindow,
            TextBox messageInputBox,
            Button sendButton,
            Button closeButton,
            StackPanel messagesPanel,
            ScrollViewer messageScrollViewer)
        {
            _minimizedButton = minimizedButton ?? throw new ArgumentNullException(nameof(minimizedButton));
            _expandedWindow = expandedWindow ?? throw new ArgumentNullException(nameof(expandedWindow));
            _messageInputBox = messageInputBox ?? throw new ArgumentNullException(nameof(messageInputBox));
            _sendButton = sendButton ?? throw new ArgumentNullException(nameof(sendButton));
            _messagesPanel = messagesPanel ?? throw new ArgumentNullException(nameof(messagesPanel));
            _messageScrollViewer = messageScrollViewer ?? throw new ArgumentNullException(nameof(messageScrollViewer));

            // Wire up events
            _minimizedButton.Click += OnMinimizedButtonClick;
            closeButton.Click += OnCloseButtonClick;
            _sendButton.Click += OnSendButtonClick;
            _messageInputBox.KeyDown += OnMessageInputBoxKeyDown;
        }

        private void OnMinimizedButtonClick(object sender, RoutedEventArgs e)
        {
            ExpandChatWindow();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            MinimizeChatWindow();
        }

        private async void OnSendButtonClick(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void OnMessageInputBoxKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
                bool isShiftPressed = (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;

                if (!isShiftPressed)
                {
                    e.Handled = true;
                    await SendMessageAsync();
                }
            }
        }

        private void ExpandChatWindow()
        {
            _minimizedButton.Visibility = Visibility.Collapsed;
            _expandedWindow.Visibility = Visibility.Visible;
            _messageInputBox.Focus(FocusState.Programmatic);
        }

        private void MinimizeChatWindow()
        {
            _expandedWindow.Visibility = Visibility.Collapsed;
            _minimizedButton.Visibility = Visibility.Visible;
        }

        private async Task SendMessageAsync()
        {
            var message = _messageInputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return;

            _messageInputBox.Text = string.Empty;

            AddUserMessage(message);

            _sendButton.IsEnabled = false;
            _messageInputBox.IsEnabled = false;

            try
            {
                await Task.Delay(1000);
                
                var response = GetBotResponse(message);
                AddBotMessage(response);
            }
            catch (Exception ex)
            {
                AddBotMessage($"Sorry, an error occurred: {ex.Message}");
            }
            finally
            {
                _sendButton.IsEnabled = true;
                _messageInputBox.IsEnabled = true;
                _messageInputBox.Focus(FocusState.Programmatic);
            }
        }

        private void AddUserMessage(string message)
        {
            var messageGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 212)),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(8),
                MaxWidth = 300
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.White)
            };

            border.Child = textBlock;
            messageGrid.Children.Add(border);
            _messagesPanel.Children.Add(messageGrid);

            ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            var messageGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var border = new Border
            {
                Background = (Brush)Application.Current.Resources["LayerFillColorAltBrush"],
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(8),
                MaxWidth = 300
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"]
            };

            border.Child = textBlock;
            messageGrid.Children.Add(border);
            _messagesPanel.Children.Add(messageGrid);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            _messageScrollViewer.UpdateLayout();
            _messageScrollViewer.ChangeView(null, _messageScrollViewer.ScrollableHeight, null);
        }

        private string GetBotResponse(string userMessage)
        {
            var lower = userMessage.ToLowerInvariant();

            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey") || lower.Contains("xin chào"))
                return "Hello! How can I assist you today?";

            if (lower.Contains("help") || lower.Contains("giúp"))
                return "I can help you with information about products, orders, customers, and reports. What would you like to know?";

            if (lower.Contains("product") || lower.Contains("s?n ph?m"))
                return "You can manage products in the Products section. Would you like to know more about product features?";

            if (lower.Contains("order") || lower.Contains("??n hàng"))
                return "The Orders section allows you to view and manage customer orders. Need specific information?";

            if (lower.Contains("customer") || lower.Contains("khách hàng"))
                return "You can view customer information and history in the Customers section.";

            if (lower.Contains("report") || lower.Contains("báo cáo"))
                return "The Reports section provides analytics and insights about your business.";

            if (lower.Contains("thank") || lower.Contains("c?m ?n"))
                return "You're welcome! Let me know if you need anything else.";

            return "I understand you're asking about: \"" + userMessage + "\". Could you provide more details so I can assist you better?";
        }
    }
}
