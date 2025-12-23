using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Products;
using System.Linq;

namespace TechHaven.Presentation.WinUI.Views.Order
{
    /// <summary>
    /// Dialog for selecting a product from the product list
    /// </summary>
    internal sealed class ProductSelectorDialog
    {
        private readonly XamlRoot _xamlRoot;
        private readonly DataTemplate? _productItemTemplate;

        /// <summary>
        /// Initializes a new instance of ProductSelectorDialog
        /// </summary>
        /// <param name="xamlRoot">XamlRoot for dialog display</param>
        /// <param name="productItemTemplate">Optional DataTemplate for product items</param>
        public ProductSelectorDialog(XamlRoot xamlRoot, DataTemplate? productItemTemplate = null)
        {
            _xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
            _productItemTemplate = productItemTemplate;
        }

        /// <summary>
        /// Shows the product selector dialog and returns the selected product
        /// </summary>
        /// <returns>Selected product DTO or null if cancelled</returns>
        public async Task<ProductDto?> ShowAsync()
        {
            var viewModel = new ProductViewModel();
            await viewModel.LoadProductsCommand.ExecuteAsync(null);

            var searchBox = new TextBox
            {
                PlaceholderText = "Tìm kiếm sản phẩm (tên)",
                Margin = new Thickness(0, 0, 0, 8)
            };

            var listView = new ListView
            {
                ItemsSource = viewModel.Products,
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 400
            };

            if (_productItemTemplate != null)
            {
                listView.ItemTemplate = _productItemTemplate;
            }

            // Add no results message
            var noResultsText = new TextBlock
            {
                Text = "Không tìm thấy sản phẩm nào",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 128, 128, 128)),
                Margin = new Thickness(0, 20, 0, 0),
                Visibility = Visibility.Collapsed
            };

            var taskCompletionSource = new TaskCompletionSource<ProductDto?>();

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(searchBox);
            panel.Children.Add(listView);
            panel.Children.Add(noResultsText);

            var dialog = new ContentDialog
            {
                Title = "Chọn sản phẩm",
                Content = new ScrollViewer { Content = panel, MaxHeight = 520 },
                CloseButtonText = "Hủy",
                PrimaryButtonText = "Chọn",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

            // Update no results visibility based on search results
            void UpdateNoResultsVisibility()
            {
                var hasResults = viewModel.Products.Any();
                var hasSearchTerm = !string.IsNullOrWhiteSpace(searchBox.Text);
                
                noResultsText.Visibility = !hasResults && hasSearchTerm 
                    ? Visibility.Visible 
                    : Visibility.Collapsed;
                    
                listView.Visibility = hasResults ? Visibility.Visible : Visibility.Collapsed;
                
                // Disable primary button if no results
                dialog.IsPrimaryButtonEnabled = hasResults;
            }

            viewModel.Products.CollectionChanged += (s, e) => UpdateNoResultsVisibility();

            listView.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is ProductItemViewModel productItem)
                {
                    try { dialog.Hide(); }
                    catch { }
                    taskCompletionSource.TrySetResult(productItem.Product);
                }
            };

            dialog.PrimaryButtonClick += (s, e) =>
            {
                if (listView.SelectedItem is ProductItemViewModel productItem)
                    taskCompletionSource.TrySetResult(productItem.Product);
                else
                    taskCompletionSource.TrySetResult(null);
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
        private void SetupSearchDebounce(TextBox searchBox, ProductViewModel viewModel, Action updateNoResultsVisibility)
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
