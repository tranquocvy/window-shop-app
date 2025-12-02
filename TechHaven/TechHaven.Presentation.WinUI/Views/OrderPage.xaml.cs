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
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Products;
using TechHaven.Shared.DTOs.Customers;

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

            // Use production PDF service (will include StoreLogo.png if present in output)
            DataContext = new OrderViewModel(new TechHaven.Presentation.WinUI.Helpers.OrderPdfService());
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
                    Title = $"Order Details",
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
                    Title = $"Update Status - Order",
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
                    Content = $"Are you sure you want to delete this order?\n\n" +
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
        /// Show dialog to create new order with full cart UI (multiple products)
        /// </summary>
        private async void CreateOrder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            TechHaven.Shared.DTOs.Customers.CustomerDto? selectedCustomer = null;
            var cart = new List<(TechHaven.Shared.DTOs.Products.ProductDto Product, int Quantity, decimal UnitPrice)>();

            bool openCustomerSelectorRequested = false;
            bool openProductSelectorRequested = false;

            // UI elements
            var selectCustomerBtn = new Button { Content = "Chọn khách hàng (Walk-in nếu bỏ trống)", HorizontalAlignment = HorizontalAlignment.Stretch };
            var addProductBtn = new Button { Content = "Thêm sản phẩm", HorizontalAlignment = HorizontalAlignment.Stretch };
            var cartPanel = new StackPanel { Spacing = 8 };
            var subtotalText = new TextBlock { Text = "Subtotal: 0 ₫", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
            var discountBox = new TextBox { PlaceholderText = "Discount", Text = "0", Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 0) };
            var totalText = new TextBlock { Text = "Total: 0 ₫", FontWeight = Microsoft.UI.Text.FontWeights.Bold };
            var notesBox = new TextBox { PlaceholderText = "Notes (optional)", AcceptsReturn = true, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap, Height = 80 };

            // Helper to recalc totals and refresh cart UI
            void RefreshCartUI()
            {
                cartPanel.Children.Clear();
                decimal subtotal = 0;
                foreach (var item in cart)
                {
                    var grid = new Grid { ColumnDefinitions = {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(80) },
                        new ColumnDefinition { Width = new GridLength(100) },
                        new ColumnDefinition { Width = new GridLength(40) }
                    }, Margin = new Microsoft.UI.Xaml.Thickness(0,4,0,4) };

                    var nameBlock = new StackPanel { Orientation = Orientation.Vertical };
                    nameBlock.Children.Add(new TextBlock { Text = item.Product.ProductName, TextWrapping = TextWrapping.Wrap, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                    nameBlock.Children.Add(new TextBlock { Text = item.Product.BrandName, FontSize = 12, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"], TextWrapping = TextWrapping.Wrap });

                    Grid.SetColumn(nameBlock, 0);

                    var qtyBox = new TextBox { Text = item.Quantity.ToString(), HorizontalAlignment = HorizontalAlignment.Right, Width = 60 };
                    Grid.SetColumn(qtyBox, 1);

                    var priceBox = new TextBox { Text = item.UnitPrice.ToString("N0"), HorizontalAlignment = HorizontalAlignment.Right, Width = 90, IsReadOnly = true };
                    Grid.SetColumn(priceBox, 2);

                    var removeBtn = new Button { Content = "X", Width = 36, HorizontalAlignment = HorizontalAlignment.Right };
                    Grid.SetColumn(removeBtn, 3);

                    // Capture reference to product for handler via index
                    var prod = item.Product;

                    // Handlers
                    qtyBox.TextChanged += (_, _) =>
                    {
                        if (int.TryParse(qtyBox.Text, out var q) && q > 0)
                        {
                            var idx = cart.FindIndex(ci => ci.Product.ProductId == prod.ProductId);
                            if (idx >= 0)
                            {
                                var cur = cart[idx];
                                cart[idx] = (cur.Product, q, cur.UnitPrice);
                                RefreshCartUI();
                            }
                        }
                    };

                    // Price is read-only per requirements; do not allow editing

                    removeBtn.Click += (_, _) =>
                    {
                        var idx = cart.FindIndex(ci => ci.Product.ProductId == prod.ProductId);
                        if (idx >= 0)
                        {
                            cart.RemoveAt(idx);
                            RefreshCartUI();
                        }
                    };

                    grid.Children.Add(nameBlock);
                    grid.Children.Add(qtyBox);
                    grid.Children.Add(priceBox);
                    grid.Children.Add(removeBtn);

                    cartPanel.Children.Add(grid);

                    subtotal += item.Quantity * item.UnitPrice;
                }

                subtotalText.Text = $"Subtotal: {subtotal:N0} ₫";

                if (!decimal.TryParse(discountBox.Text, out var discount) || discount < 0)
                    discount = 0;

                var total = Math.Max(0, subtotal - discount);
                totalText.Text = $"Total: {total:N0} ₫";
            }

            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = "Customer:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(selectCustomerBtn);
            panel.Children.Add(new TextBlock { Text = "Products:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(addProductBtn);
            panel.Children.Add(cartPanel);
            panel.Children.Add(subtotalText);
            panel.Children.Add(new TextBlock { Text = "Discount:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(discountBox);
            panel.Children.Add(totalText);
            panel.Children.Add(new TextBlock { Text = "Notes:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(notesBox);

            var dialog = new ContentDialog
            {
                Title = "Create New Order",
                Content = new ScrollViewer { Content = panel, MaxHeight = 600 },
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Now attach handlers that can hide the parent dialog to avoid nested dialogs
            addProductBtn.Click += (_, _) =>
            {
                openProductSelectorRequested = true;
                try { dialog.Hide(); } catch { }
            };

            selectCustomerBtn.Click += (_, _) =>
            {
                openCustomerSelectorRequested = true;
                try { dialog.Hide(); } catch { }
            };

            // Show dialog loop to allow opening selectors without nesting
            while (true)
            {
                // Show and await
                var showTask = dialog.ShowAsync();
                var result = await showTask;

                if (openCustomerSelectorRequested)
                {
                    openCustomerSelectorRequested = false;
                    var c = await ShowCustomerSelectorAsync();
                    selectedCustomer = c;
                    selectCustomerBtn.Content = c == null ? "Walk-in / None" : $"{c.CustomerName} - {c.PhoneNumber}";
                    RefreshCartUI();
                    continue; // reopen parent dialog
                }

                if (openProductSelectorRequested)
                {
                    openProductSelectorRequested = false;
                    var p = await ShowProductSelectorAsync();
                    if (p != null)
                    {
                        // Add to cart or increment quantity
                        var idx = cart.FindIndex(ci => ci.Product.ProductId == p.ProductId);
                        if (idx >= 0)
                        {
                            var cur = cart[idx];
                            cart[idx] = (cur.Product, cur.Quantity + 1, cur.UnitPrice);
                        }
                        else
                        {
                            cart.Add((p, 1, p.SellPrice));
                        }
                    }
                    RefreshCartUI();
                    continue;
                }

                // If user clicked Create
                if (result == ContentDialogResult.Primary)
                {
                    if (!cart.Any())
                    {
                        await ShowErrorDialog("Validation Error", "Please add at least one product to the order.");
                        return;
                    }

                    // parse discount
                    if (!decimal.TryParse(discountBox.Text, out var discount) || discount < 0)
                        discount = 0;

                    // build DTO
                    var items = cart.Select(ci => new OrderUpsertItemDto
                    {
                        ProductId = ci.Product.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.UnitPrice
                    }).ToList();

                    var createDto = new OrderUpsertRequestDto
                    {
                        CustomerId = selectedCustomer?.CustomerId,
                        Discount = discount,
                        Notes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim(),
                        Items = items
                    };

                    await ViewModel.CreateOrderCommand.ExecuteAsync(createDto);
                    await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
                }

                break;
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
            panel.Children.Add(new TextBlock { Text = $"Order", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold });
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

        private async void PrintOrder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                if (ViewModel?.PrintOrderCommand != null && ViewModel.PrintOrderCommand.CanExecute(item))
                {
                    await ViewModel.PrintOrderCommand.ExecuteAsync(item);
                }
            }
        }

        // Show a modal customer selector that reuses CustomerViewModel (calls API)
        private async System.Threading.Tasks.Task<TechHaven.Shared.DTOs.Customers.CustomerDto?> ShowCustomerSelectorAsync()
        {
            var vm = new CustomerViewModel();
            await vm.EnsureInitialLoadAsync();

            var searchBox = new TextBox { PlaceholderText = "Tìm kiếm khách hàng (tên, số điện thoại)", Margin = new Microsoft.UI.Xaml.Thickness(0,0,0,8) };

            var list = new ListView
            {
                ItemsSource = vm.Customers,
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 360
            };

            // Use predefined DataTemplate in XAML resources for customer items
            if (this.Resources.ContainsKey("CustomerItemTemplate") && this.Resources["CustomerItemTemplate"] is DataTemplate tpl)
            {
                list.ItemTemplate = tpl;
            }

            var tcs = new System.Threading.Tasks.TaskCompletionSource<TechHaven.Shared.DTOs.Customers.CustomerDto?>();

            // Build panel first
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(searchBox);
            panel.Children.Add(list);

            var dialog = new ContentDialog
            {
                Title = "Chọn khách hàng",
                Content = new ScrollViewer { Content = panel, MaxHeight = 480 },
                CloseButtonText = "Hủy",
                PrimaryButtonText = "Chọn",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            // Item click should set result and close dialog
            list.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TechHaven.Shared.DTOs.Customers.CustomerDto c)
                {
                    // Close dialog first to avoid async reentrancy issues then set result
                    try
                    {
                        dialog.Hide();
                    }
                    catch { }
                    tcs.TrySetResult(c);
                }
            };

            // When user presses primary but no selection via click, use selected item
            dialog.PrimaryButtonClick += (s, e) =>
            {
                if (list.SelectedItem is TechHaven.Shared.DTOs.Customers.CustomerDto c)
                {
                    tcs.TrySetResult(c);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            };

            // If user cancels or dialog closed without selection, return null
            dialog.Closed += (s, e) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(null);
            };

            searchBox.TextChanged += (s, e) =>
            {
                vm.SearchTerm = searchBox.Text;
            };

            // Keep dialog responsive by hooking vm collection to UI; ensure initial load already done
            panel.DataContext = vm;

            // Show dialog and wait for selection via TaskCompletionSource
            var showTask = dialog.ShowAsync();
            var result = await tcs.Task;

            // Ensure ShowAsync completes (dialog closed)
            try { await showTask; } catch { }

            return result;
        }

        // Show a modal product selector that reuses ProductViewModel (calls API)
        private async System.Threading.Tasks.Task<TechHaven.Shared.DTOs.Products.ProductDto?> ShowProductSelectorAsync()
        {
            var vm = new ProductViewModel();
            // Use generated command to load products
            await vm.LoadProductsCommand.ExecuteAsync(null);

            var searchBox = new TextBox { PlaceholderText = "Tìm kiếm sản phẩm (tên)", Margin = new Microsoft.UI.Xaml.Thickness(0,0,0,8) };

            var list = new ListView
            {
                ItemsSource = vm.Products,
                IsItemClickEnabled = true,
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 400
            };

            // Use predefined DataTemplate in XAML resources for product items
            if (this.Resources.ContainsKey("ProductItemTemplate") && this.Resources["ProductItemTemplate"] is DataTemplate tpl)
            {
                list.ItemTemplate = tpl;
            }

            var tcs = new System.Threading.Tasks.TaskCompletionSource<TechHaven.Shared.DTOs.Products.ProductDto?>();

            // Build panel
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(searchBox);
            panel.Children.Add(list);

            var dialog = new ContentDialog
            {
                Title = "Chọn sản phẩm",
                Content = new ScrollViewer { Content = panel, MaxHeight = 520 },
                CloseButtonText = "Hủy",
                PrimaryButtonText = "Chọn",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            list.ItemClick += (s, e) =>
            {
                if (e.ClickedItem is TechHaven.Presentation.WinUI.ViewModel.ProductItemViewModel vmItem)
                {
                    try { dialog.Hide(); } catch { }
                    tcs.TrySetResult(vmItem.Product);
                }
            };

            dialog.PrimaryButtonClick += (s, e) =>
            {
                if (list.SelectedItem is TechHaven.Presentation.WinUI.ViewModel.ProductItemViewModel vmItem)
                    tcs.TrySetResult(vmItem.Product);
                else
                    tcs.TrySetResult(null);
            };

            dialog.Closed += (s, e) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(null);
            };

            searchBox.TextChanged += (s, e) =>
            {
                vm.SearchTerm = searchBox.Text;
            };

            panel.DataContext = vm;

            var showTask = dialog.ShowAsync();
            var result = await tcs.Task;
            try { await showTask; } catch { }
            return result;
        }
    }
}
