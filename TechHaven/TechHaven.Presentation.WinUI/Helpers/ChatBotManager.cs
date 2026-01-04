using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Windows.System;
using TechHaven.Shared.DTOs.AI;

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
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessageDto> _conversationHistory;

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

            _httpClient = ApiClientFactory.GetHttpClient();
            _conversationHistory = new List<ChatMessageDto>();

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

            var userMessageTime = DateTime.UtcNow;
            AddUserMessage(message, userMessageTime);

            var userMessage = new ChatMessageDto
            {
                Role = Role.User,
                Content = message,
                Timestamp = userMessageTime
            };
            _conversationHistory.Add(userMessage);

            _sendButton.IsEnabled = false;
            _messageInputBox.IsEnabled = false;

            try
            {
                var response = await GetAIResponseAsync(message);
                var botMessageTime = DateTime.UtcNow;
                AddBotMessage(response, botMessageTime);

                var assistantMessage = new ChatMessageDto
                {
                    Role = Role.Assistance,
                    Content = response,
                    Timestamp = botMessageTime
                };
                _conversationHistory.Add(assistantMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Sorry, an error occurred: {ex.Message}";
                AddBotMessage(errorMessage, DateTime.UtcNow);
            }
            finally
            {
                _sendButton.IsEnabled = true;
                _messageInputBox.IsEnabled = true;
                _messageInputBox.Focus(FocusState.Programmatic);
            }
        }

        private void AddUserMessage(string message, DateTime timestamp)
        {
            var messageGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var stackPanel = new StackPanel
            {
                Spacing = 4
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
            stackPanel.Children.Add(border);

            var timestampText = new TextBlock
            {
                Text = timestamp.ToLocalTime().ToString("HH:mm"),
                FontSize = 10,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 4, 0)
            };
            stackPanel.Children.Add(timestampText);

            messageGrid.Children.Add(stackPanel);
            _messagesPanel.Children.Add(messageGrid);

            ScrollToBottom();
        }

        private void AddBotMessage(string message, DateTime timestamp)
        {
            var messageGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var stackPanel = new StackPanel
            {
                Spacing = 4
            };

            var border = new Border
            {
                Background = (Brush)Application.Current.Resources["LayerFillColorAltBrush"],
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(8),
                MaxWidth = 400
            };

            // Parse markdown content from AI response
            // Supports: Headers, Bold, Italic, Code, Tables, Blockquotes, Lists, etc.
            // Example AI response:
            //   # Analysis
            //   | Product | Stock |
            //   |---------|-------|
            //   | iPhone  | 50    |
            //   > **Note**: Stock is good!
            var markdownContent = MarkdownHelper.ParseMarkdown(message);

            border.Child = markdownContent;
            stackPanel.Children.Add(border);

            var timestampText = new TextBlock
            {
                Text = timestamp.ToLocalTime().ToString("HH:mm"),
                FontSize = 10,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 2, 0, 0)
            };
            stackPanel.Children.Add(timestampText);

            messageGrid.Children.Add(stackPanel);
            _messagesPanel.Children.Add(messageGrid);

            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            _messageScrollViewer.UpdateLayout();
            _messageScrollViewer.ChangeView(null, _messageScrollViewer.ScrollableHeight, null);
        }

        private async Task<string> GetAIResponseAsync(string userMessage)
        {
            try
            {
                var request = new ChatRequestDto
                {
                    Message = userMessage,
                    History = _conversationHistory.Count > 0 ? new List<ChatMessageDto>(_conversationHistory) : null
                };

                var response = await _httpClient.PostAsJsonAsync("api/AI/chat", request);
                var result = await response.EnsureSuccessAndReadWrapperAsync<ChatResponseDto>("Failed to get AI response");

                if (result.Success && result.Data != null)
                {
                    return result.Data.Response;
                }
                else
                {
                    var errorMessage = result.Message ?? "Failed to get AI response";
                    if (result.Errors != null && result.Errors.Count > 0)
                    {
                        errorMessage += ": " + string.Join(", ", result.Errors);
                    }
                    return $"Sorry, I couldn't process your request. {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                return $"Sorry, I encountered an error: {ex.Message}";
            }
        }
    }
}
