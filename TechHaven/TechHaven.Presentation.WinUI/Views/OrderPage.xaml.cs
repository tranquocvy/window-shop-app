using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Presentation.WinUI.Views.Controls;
using TechHaven.Shared.DTOs.Orders;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// Order management page with search, filter, and CRUD operations.
    /// </summary>
    public sealed partial class OrderPage : Page
    {
        private OrderViewModel ViewModel => (OrderViewModel)DataContext;

        public OrderPage()
        {
            InitializeComponent();
        }

        private async void OrdersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is OrderItemViewModel item)
            {
                // Show edit dialog
                await ShowEditOrderDialog(item);
            }
        }

        private async void ViewDetails_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                var details = item.Order.Details ?? new List<OrderDetailDto>();
                var itemsText = string.Join("\n", details.Select(d => 
                    $"- {d.ProductName} (ID: {d.ProductId}): {d.Quantity} × {d.UnitPrice:N0} ? = {d.SubTotal:N0} ?"));

                var dialog = new ContentDialog
                {
                    Title = $"Order Details #{item.Order.OrderId}",
                    Content = new ScrollViewer
                    {
                        Content = new TextBlock
                        {
                            Text = $"Customer: {item.CustomerDisplay}\n" +
                                  $"User: {item.Order.UserFullName}\n" +
                                  $"Date: {item.OrderDateDisplay}\n" +
                                  $"Status: {item.StatusDisplay}\n\n" +
                                  $"Items:\n{itemsText}\n\n" +
                                  $"Subtotal: {item.Order.SubtotalAmount:N0} ?\n" +
                                  $"Discount: {item.Order.Discount:N0} ?\n" +
                                  $"Total: {item.TotalAmountDisplay}\n\n" +
                                  $"Notes: {item.Order.Notes ?? "(none)"}",
                            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
                        },
                        MaxHeight = 500
                    },
                    CloseButtonText = "Close",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void UpdateStatus_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                // Simple status update dialog
                var statusComboBox = new ComboBox
                {
                    PlaceholderText = "Select new status",
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                    Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0)
                };

                statusComboBox.Items.Add(new ComboBoxItem { Content = "Pending", Tag = OrderStatus.Pending });
                statusComboBox.Items.Add(new ComboBoxItem { Content = "Processing", Tag = OrderStatus.Processing });
                statusComboBox.Items.Add(new ComboBoxItem { Content = "Completed", Tag = OrderStatus.Completed });
                statusComboBox.Items.Add(new ComboBoxItem { Content = "Cancelled", Tag = OrderStatus.Cancelled });
                statusComboBox.Items.Add(new ComboBoxItem { Content = "Returned", Tag = OrderStatus.Returned });

                // Set current status
                var currentItem = statusComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => (OrderStatus)i.Tag == item.Order.Status);
                if (currentItem != null)
                    statusComboBox.SelectedItem = currentItem;

                var panel = new StackPanel();
                panel.Children.Add(new TextBlock
                {
                    Text = $"Current status: {item.StatusDisplay}",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                });
                panel.Children.Add(statusComboBox);

                var dialog = new ContentDialog
                {
                    Title = $"Update Status - Order #{item.Order.OrderId}",
                    Content = panel,
                    PrimaryButtonText = "Update",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && statusComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var newStatus = (OrderStatus)selectedItem.Tag;

                    var dto = new OrderUpsertRequestDto
                    {
                        Status = newStatus
                    };

                    await ViewModel.UpdateOrderAsync(item.Order.OrderId, dto);
                    await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
                }
            }
        }

        private async void DeleteOrder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirm Delete",
                    Content = $"Are you sure you want to delete order #{item.Order.OrderId}?\n\n" +
                             $"Customer: {item.CustomerDisplay}\n" +
                             $"Total: {item.TotalAmountDisplay}",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.DeleteOrderCommand.ExecuteAsync(item);
                }
            }
        }

        // Filter event handlers to trigger search when filters change
        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null && sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                await ViewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        private async void FromDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // Trigger reload regardless of whether date is set or cleared
            if (ViewModel != null)
            {
                await ViewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        private async void ToDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // Trigger reload regardless of whether date is set or cleared
            if (ViewModel != null)
            {
                await ViewModel.SearchCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Show dialog to create new order
        /// </summary>
        private async void CreateOrder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Simple create form
            var customerIdBox = new TextBox { PlaceholderText = "Customer ID (optional)", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var productIdBox = new TextBox { PlaceholderText = "Product ID", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var quantityBox = new TextBox { PlaceholderText = "Quantity", Text = "1", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var priceBox = new TextBox { PlaceholderText = "Unit Price", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var discountBox = new TextBox { PlaceholderText = "Discount", Text = "0", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var notesBox = new TextBox
            {
                PlaceholderText = "Notes (optional)",
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 60,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0)
            };

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = "Customer ID:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(customerIdBox);
            panel.Children.Add(new TextBlock { Text = "Product ID:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(productIdBox);
            panel.Children.Add(new TextBlock { Text = "Quantity:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(quantityBox);
            panel.Children.Add(new TextBlock { Text = "Unit Price:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(priceBox);
            panel.Children.Add(new TextBlock { Text = "Discount:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(discountBox);
            panel.Children.Add(new TextBlock { Text = "Notes:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(notesBox);

            var dialog = new ContentDialog
            {
                Title = "Create New Order",
                Content = new ScrollViewer { Content = panel, MaxHeight = 500 },
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Validate and create
                if (!int.TryParse(productIdBox.Text, out var productId) || productId <= 0)
                {
                    await ShowErrorDialog("Validation Error", "Product ID must be a valid number greater than 0.");
                    return;
                }

                if (!int.TryParse(quantityBox.Text, out var quantity) || quantity <= 0)
                {
                    await ShowErrorDialog("Validation Error", "Quantity must be a valid number greater than 0.");
                    return;
                }

                if (!decimal.TryParse(priceBox.Text, out var price) || price < 0)
                {
                    await ShowErrorDialog("Validation Error", "Unit price must be a valid non-negative number.");
                    return;
                }

                if (!decimal.TryParse(discountBox.Text, out var discount) || discount < 0)
                {
                    discount = 0;
                }

                int? customerId = null;
                if (!string.IsNullOrWhiteSpace(customerIdBox.Text))
                {
                    if (int.TryParse(customerIdBox.Text, out var custId))
                        customerId = custId;
                }

                var createDto = new OrderUpsertRequestDto
                {
                    CustomerId = customerId,
                    Discount = discount,
                    Notes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim(),
                    Items = new List<OrderUpsertItemDto>
                    {
                        new OrderUpsertItemDto
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            UnitPrice = price
                        }
                    }
                };

                // Call create and reload
                await ViewModel.CreateOrderCommand.ExecuteAsync(createDto);
                await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
            }
        }

        /// <summary>
        /// Show dialog to edit existing order (simplified - just notes and discount)
        /// </summary>
        private async System.Threading.Tasks.Task ShowEditOrderDialog(OrderItemViewModel item)
        {
            var discountBox = new TextBox
            {
                Text = item.Order.Discount.ToString(),
                PlaceholderText = "Discount",
                Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0)
            };

            var notesBox = new TextBox
            {
                Text = item.Order.Notes ?? string.Empty,
                PlaceholderText = "Notes",
                AcceptsReturn = true,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                Height = 80,
                Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0)
            };

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = $"Order #{item.Order.OrderId}", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
            panel.Children.Add(new TextBlock { Text = $"Customer: {item.CustomerDisplay}" });
            panel.Children.Add(new TextBlock { Text = $"Current Total: {item.TotalAmountDisplay}" });
            panel.Children.Add(new TextBlock { Text = "Discount:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Microsoft.UI.Xaml.Thickness(0, 8, 0, 0) });
            panel.Children.Add(discountBox);
            panel.Children.Add(new TextBlock { Text = "Notes:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(notesBox);

            var dialog = new ContentDialog
            {
                Title = "Edit Order (Basic Info)",
                Content = panel,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await ShowErrorDialog("Info", "Full order editing is not yet implemented.\n\n" +
                                              "Currently, you can only update the status using the 'Update Status' option in the context menu.");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
