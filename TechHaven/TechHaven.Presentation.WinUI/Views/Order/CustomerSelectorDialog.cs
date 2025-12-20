using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Customers;

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

            var taskCompletionSource = new TaskCompletionSource<CustomerDto?>();

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(searchBox);
            panel.Children.Add(listView);

            var dialog = new ContentDialog
            {
                Title = "Chọn khách hàng",
                Content = new ScrollViewer { Content = panel, MaxHeight = 480 },
                CloseButtonText = "Hủy",
                PrimaryButtonText = "Chọn",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

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

            SetupSearchDebounce(searchBox, viewModel);
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
        private void SetupSearchDebounce(TextBox searchBox, CustomerViewModel viewModel)
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
                        });
                    }
                    catch (TaskCanceledException) { }
                    catch { }
                });
            };
        }
    }
}
