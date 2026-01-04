using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Customers;
using System.Linq;

namespace TechHaven.Presentation.WinUI.Views.Order
{
    /// <summary>
    /// Dialog for selecting a customer from the customer list
    /// </summary>
    internal sealed class CustomerSelectorDialog
    {
        private readonly XamlRoot _xamlRoot;
        private readonly DataTemplate? _customerItemTemplate;

        /// <summary>
        /// Initializes a new instance of CustomerSelectorDialog
        /// </summary>
        /// <param name="xamlRoot">XamlRoot for dialog display</param>
        /// <param name="customerItemTemplate">Optional DataTemplate for customer items</param>
        public CustomerSelectorDialog(XamlRoot xamlRoot, DataTemplate? customerItemTemplate = null)
        {
            _xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
            _customerItemTemplate = customerItemTemplate;
        }

        /// <summary>
        /// Shows the customer selector dialog and returns the selected customer
        /// </summary>
        /// <returns>Selected customer DTO or null if cancelled</returns>
        public async Task<CustomerDto?> ShowAsync()
        {
            var viewModel = new CustomerViewModel();
            await viewModel.EnsureInitialLoadAsync();

            var searchBox = new TextBox
            {
                PlaceholderText = "Tìm kiếm khách hàng (tên, số điện thoại)",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var listView = new ListView
            {
                ItemsSource = viewModel.Customers,
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 360
            };

            if (_customerItemTemplate != null)
            {
                listView.ItemTemplate = _customerItemTemplate;
            }

            // Add no results message
            var noResultsText = new TextBlock
            {
                Text = "Không tìm thấy khách hàng nào",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                Margin = new Thickness(0, 20, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var taskCompletionSource = new TaskCompletionSource<CustomerDto?>();

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(searchBox);
            panel.Children.Add(listView);
            panel.Children.Add(noResultsText);

            var dialog = new ContentDialog
            {
                Title = "Chọn khách hàng",
                Content = new ScrollViewer { Content = panel, MaxHeight = 480 },
                CloseButtonText = "Hủy",
                PrimaryButtonText = "Chọn",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

            // Update no results visibility based on search results
            void UpdateNoResultsVisibility()
            {
                var hasResults = viewModel.Customers.Any();
                var hasSearchTerm = !string.IsNullOrWhiteSpace(searchBox.Text);
                
                noResultsText.Visibility = !hasResults && hasSearchTerm 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
                    
                listView.Visibility = hasResults ? Visibility.Visible : Visibility.Collapsed;
                
                // Disable primary button if no results
                dialog.IsPrimaryButtonEnabled = hasResults;
            }

            viewModel.Customers.CollectionChanged += (s, e) => UpdateNoResultsVisibility();

            listView.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is CustomerDto customer)
                {
                    try { dialog.Hide(); }
                    catch { }
                    taskCompletionSource.TrySetResult(customer);
                }
            };

            dialog.PrimaryButtonClick += (s, e) =>
            {
                taskCompletionSource.TrySetResult(listView.SelectedItem as CustomerDto);
            };

            dialog.Closed += (s, e) =>
            {
                if (!taskCompletionSource.Task.IsCompleted)
                    taskCompletionSource.TrySetResult(null);
            };

            SetupSearchDebounce(searchBox, viewModel, UpdateNoResultsVisibility);
            panel.DataContext = viewModel;

            var showTask = dialog.ShowAsync();
            var result = await taskCompletionSource.Task;

            try { await showTask; }
            catch { }

            return result;
        }

        /// <summary>
        /// Sets up debounced search functionality
        /// </summary>
        private void SetupSearchDebounce(TextBox searchBox, CustomerViewModel viewModel, Action updateNoResultsVisibility)
        {
            CancellationTokenSource? searchCts = null;

            searchBox.TextChanged += (s, e) =>
            {
                searchCts?.Cancel();
                searchCts = new CancellationTokenSource();
                var token = searchCts.Token;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(500, token);
                        if (token.IsCancellationRequested) return;

                        _ = App.MainWindow?.DispatcherQueue.TryEnqueue(() =>
                        {
                            viewModel.SearchTerm = searchBox.Text;
                            updateNoResultsVisibility();
                        });
                    }
                    catch (TaskCanceledException) { }
                    catch { }
                });
            };
        }
    }
}
