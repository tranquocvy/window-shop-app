using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Views.Order;
using System;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// Order management page with search, filter, and CRUD operations
    /// </summary>
    public sealed partial class OrderPage : Page
    {
        #region Fields

        private readonly OrderViewModel _viewModel;

        #endregion

        #region Properties

        private OrderViewModel ViewModel => _viewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of OrderPage
        /// </summary>
        public OrderPage()
        {
            InitializeComponent();
            
            _viewModel = new OrderViewModel(new OrderPdfService());
            DataContext = _viewModel;
        }

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// Handles navigation to this page
        /// </summary>
        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                _viewModel.PageSize = AppState.PageSize;
            }
            catch { }

            _ = _viewModel.LoadOrdersCommand.ExecuteAsync(null);
        }

        #endregion

        #region Event Handlers - List Interactions

        /// <summary>
        /// Handles order item click to show edit dialog
        /// </summary>
        private async void OrdersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is OrderItemViewModel item)
            {
                await ShowEditOrderDialogAsync(item);
            }
        }

        /// <summary>
        /// Handles view details context menu click
        /// </summary>
        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                await ShowEditOrderDialogAsync(item);
            }
        }

        /// <summary>
        /// Handles update status context menu click
        /// </summary>
        private async void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                var dialog = new StatusUpdateDialog(Content.XamlRoot, _viewModel);
                var success = await dialog.ShowAsync(item);
                
                if (success)
                {
                    await _viewModel.LoadOrdersCommand.ExecuteAsync(null);
                }
            }
        }

        /// <summary>
        /// Handles delete order context menu click
        /// </summary>
        private async void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                // Only allow delete for Pending orders
                if (item.Order.Status != TechHaven.Shared.DTOs.Orders.OrderStatus.Pending)
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "Cannot Delete",
                        Content = "Only Pending orders can be deleted.\nCurrent status: " + item.StatusDisplay,
                        CloseButtonText = "Close",
                        XamlRoot = Content.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var confirmDialog = new ContentDialog
                {
                    Title = "Confirm Delete",
                    Content = $"Are you sure you want to delete this order?\n\n" +
                             $"Customer: {item.CustomerDisplay}\n" +
                             $"Total: {item.TotalAmountDisplay}",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await _viewModel.DeleteOrderCommand.ExecuteAsync(item);
                }
            }
        }

        /// <summary>
        /// Handles print order context menu click
        /// </summary>
        private async void PrintOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                if (_viewModel.PrintOrderCommand?.CanExecute(item) == true)
                {
                    await _viewModel.PrintOrderCommand.ExecuteAsync(item);
                }
            }
        }

        #endregion

        #region Event Handlers - Filters

        /// <summary>
        /// Handles status filter selection change
        /// </summary>
        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                await _viewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Handles from date filter change
        /// </summary>
        private async void FromDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (_viewModel != null)
            {
                await _viewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Handles to date filter change
        /// </summary>
        private async void ToDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (_viewModel != null)
            {
                await _viewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        #endregion

        #region Event Handlers - Actions

        /// <summary>
        /// Handles create order button click
        /// </summary>
        private async void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            await ShowCreateOrderDialogAsync();
        }

        #endregion

        #region Private Methods - Dialog Display

        /// <summary>
        /// Shows the create order dialog
        /// </summary>
        private async Task ShowCreateOrderDialogAsync()
        {
            var customerTemplate = Resources.ContainsKey("CustomerItemTemplate") 
                ? Resources["CustomerItemTemplate"] as DataTemplate 
                : null;
            
            var productTemplate = Resources.ContainsKey("ProductItemTemplate") 
                ? Resources["ProductItemTemplate"] as DataTemplate 
                : null;

            var dialog = new OrderEditorDialog(
                Content.XamlRoot,
                _viewModel,
                customerTemplate,
                productTemplate);

            await dialog.ShowCreateAsync();
        }

        /// <summary>
        /// Shows the edit order dialog
        /// </summary>
        private async Task ShowEditOrderDialogAsync(OrderItemViewModel item)
        {
            if (item == null) return;

            var customerTemplate = Resources.ContainsKey("CustomerItemTemplate") 
                ? Resources["CustomerItemTemplate"] as DataTemplate 
                : null;
            
            var productTemplate = Resources.ContainsKey("ProductItemTemplate") 
                ? Resources["ProductItemTemplate"] as DataTemplate 
                : null;

            var dialog = new OrderEditorDialog(
                Content.XamlRoot,
                _viewModel,
                customerTemplate,
                productTemplate);

            await dialog.ShowEditAsync(item);
        }

        #endregion
    }
}
