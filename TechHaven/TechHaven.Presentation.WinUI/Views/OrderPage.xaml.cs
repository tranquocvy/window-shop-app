using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Presentation.WinUI.Helpers;

namespace TechHaven.Presentation.WinUI.Views
{
    /// <summary>
    /// Order management page with search, filter, and CRUD operations.
    /// </summary>
    public sealed partial class OrderPage : Page
    {
        private OrderViewModel ViewModel => (OrderViewModel)DataContext;

        private class EditItem
        {
            public int ProductId { get; set; }
            public string? ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }

        public OrderPage()
        {
            InitializeComponent();
            // Use production PDF service
            DataContext = new OrderViewModel(new TechHaven.Presentation.WinUI.Helpers.OrderPdfService());
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                ViewModel.PageSize = AppState.PageSize;
            }
            catch { }

            // Trigger load using current state
            _ = ViewModel.LoadOrdersCommand.ExecuteAsync(null);
        }

        private async void OrdersList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is OrderItemViewModel item)
            {
                // Open the edit/detail dialog (editable fields per requirements)
                await ShowEditOrderDialog(item);
            }
        }

        private async void ViewDetails_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.DataContext is OrderItemViewModel item)
            {
                await ShowEditOrderDialog(item);
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

                    var fullOrder = await ViewModel.GetOrderByIdAsync(item.Order.OrderId);
                    if (fullOrder == null)
                    {
                        await ShowErrorDialog("Load Error", "Không thể tải đơn hàng để cập nhật trạng thái.");
                        return;
                    }

                    var dto = new OrderUpsertRequestDto
                    {
                        CustomerId = fullOrder.CustomerId,
                        Discount = fullOrder.Discount,
                        Notes = fullOrder.Notes,
                        Status = newStatus,
                        Items = fullOrder.Details?.Select(d => new OrderUpsertItemDto
                        {
                            ProductId = d.ProductId,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        }).ToList() ?? new List<OrderUpsertItemDto>()
                    };

                    var resp = await ViewModel.UpdateOrderAsync(item.Order.OrderId, dto);
                    if (resp != null && resp.Success)
                    {
                        await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        var msg = resp?.Message ?? "Cập nhật trạng thái không thành công. Vui lòng thử lại hoặc kiểm tra kết nối.";
                        await ShowErrorDialog("Update Failed", msg);
                    }
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
                    var grid = new Grid
                    {
                        ColumnDefinitions = {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(80) },
                        new ColumnDefinition { Width = new GridLength(100) },
                        new ColumnDefinition { Width = new GridLength(40) }
                    },
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 4)
                    };

                    var nameBlock = new StackPanel { Orientation = Orientation.Vertical };
                    nameBlock.Children.Add(new TextBlock { Text = item.Product.ProductName, TextWrapping = TextWrapping.Wrap, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });

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
        /// Show dialog to edit existing order
        /// </summary>
        private async System.Threading.Tasks.Task ShowEditOrderDialog(OrderItemViewModel item)
        {
            if (item == null) return;

            // Load fresh full order
            var full = await ViewModel.GetOrderByIdAsync(item.Order.OrderId);
            if (full == null)
            {
                await ShowErrorDialog("Load Error", "Không thể tải chi tiết đơn hàng.");
                return;
            }

            var editableItems = full.Details?.Select(d => new EditItem
            {
                ProductId = d.ProductId,
                ProductName = d.ProductName,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity
            }).ToList() ?? new List<EditItem>();

            var itemsPanel = new StackPanel { Spacing = 8 };

            var subtotalText = new TextBlock { Text = $"Subtotal: {full.SubtotalAmount:N0} ₫", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold };
            var totalText = new TextBlock { Text = $"Total: {full.TotalAmount:N0} ₫", FontWeight = Microsoft.UI.Text.FontWeights.Bold };

            var discountCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            for (int p = 0; p <= 100; p++)
            {
                discountCombo.Items.Add(new ComboBoxItem { Content = $"{p}%", Tag = p });
            }
            int initialPct = 0;
            try
            {
                if (full.Discount > 0 && full.Discount <= 1)
                {
                    initialPct = (int)Math.Round((double)(full.Discount * 100));
                }
                else if (full.SubtotalAmount > 0)
                {
                    initialPct = (int)Math.Round((double)(full.Discount / full.SubtotalAmount * 100));
                }

                if (initialPct < 0) initialPct = 0;
                if (initialPct > 100) initialPct = 100;
            }
            catch
            {
                initialPct = 0;
            }

            if (initialPct >= 0 && initialPct <= 100)
                discountCombo.SelectedIndex = initialPct;

            var notesBox = new TextBox { Text = full.Notes ?? string.Empty, PlaceholderText = "Notes (optional)", AcceptsReturn = true, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap, Height = 80 };

            void RefreshItemsUI()
            {
                itemsPanel.Children.Clear();
                decimal subtotal = 0;

                foreach (var it in editableItems.ToList())
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = {
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = new GridLength(80) },
                            new ColumnDefinition { Width = new GridLength(100) },
                            new ColumnDefinition { Width = new GridLength(40) }
                        },
                        Margin = new Microsoft.UI.Xaml.Thickness(0, 4, 0, 4)
                    };

                    var nameBlock = new StackPanel { Orientation = Orientation.Vertical };
                    nameBlock.Children.Add(new TextBlock { Text = it.ProductName, TextWrapping = TextWrapping.Wrap, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
                    Grid.SetColumn(nameBlock, 0);

                    var qtyBox = new TextBox { Text = it.Quantity.ToString(), HorizontalAlignment = HorizontalAlignment.Right, Width = 70 };
                    Grid.SetColumn(qtyBox, 1);

                    var priceBlock = new TextBlock { Text = it.UnitPrice.ToString("N0") + " ₫", HorizontalAlignment = HorizontalAlignment.Right };
                    Grid.SetColumn(priceBlock, 2);

                    var removeBtn = new Button { Content = "X", Width = 36, HorizontalAlignment = HorizontalAlignment.Right };
                    Grid.SetColumn(removeBtn, 3);

                    var capturedId = it.ProductId;

                    qtyBox.KeyDown += (s, e) =>
                    {
                        try
                        {
                            if (e.Key == Windows.System.VirtualKey.Enter)
                            {
                                if (int.TryParse(qtyBox.Text, out var q) && q >= 0)
                                {
                                    var idx = editableItems.FindIndex(ei => ei.ProductId == capturedId);
                                    if (idx >= 0)
                                    {
                                        editableItems[idx].Quantity = q;
                                        RefreshItemsUI();
                                    }
                                }
                            }
                        }
                        catch { }
                    };

                    qtyBox.LostFocus += (_, _) =>
                    {
                        if (int.TryParse(qtyBox.Text, out var q) && q >= 0)
                        {
                            var idx = editableItems.FindIndex(ei => ei.ProductId == capturedId);
                            if (idx >= 0)
                            {
                                editableItems[idx].Quantity = q;
                                RefreshItemsUI();
                            }
                        }
                    };

                    removeBtn.Click += (_, _) =>
                    {
                        var idx = editableItems.FindIndex(ei => ei.ProductId == capturedId);
                        if (idx >= 0)
                        {
                            editableItems.RemoveAt(idx);
                            RefreshItemsUI();
                        }
                    };

                    grid.Children.Add(nameBlock);
                    grid.Children.Add(qtyBox);
                    grid.Children.Add(priceBlock);
                    grid.Children.Add(removeBtn);

                    itemsPanel.Children.Add(grid);

                    subtotal += it.UnitPrice * it.Quantity;
                }

                subtotalText.Text = $"Subtotal: {subtotal:N0} ₫";

                var pct = 0;
                if (discountCombo.SelectedItem is ComboBoxItem cbi && cbi.Tag is int t)
                    pct = t;
                var discFraction = Math.Max(0, Math.Min(100, pct)) / 100m;

                var total = Math.Max(0, subtotal - (subtotal * discFraction));
                totalText.Text = $"Total: {total:N0} ₫";
            }

            // Status combobox
            var statusCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            statusCombo.Items.Add(new ComboBoxItem { Content = "Pending", Tag = OrderStatus.Pending });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Processing", Tag = OrderStatus.Processing });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Completed", Tag = OrderStatus.Completed });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Cancelled", Tag = OrderStatus.Cancelled });
            statusCombo.Items.Add(new ComboBoxItem { Content = "Returned", Tag = OrderStatus.Returned });

            var currentStatusItem = statusCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => (OrderStatus)i.Tag == full.Status);
            if (currentStatusItem != null) statusCombo.SelectedItem = currentStatusItem;

            // Build panel
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = $"Customer: {full.CustomerName ?? "Walk-in Customer"}", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"] });
            panel.Children.Add(new TextBlock { Text = $"User: {full.UserFullName}", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"] });
            panel.Children.Add(new TextBlock { Text = $"Date: {full.OrderDate:dd/MM/yyyy HH:mm}", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"] });

            panel.Children.Add(new TextBlock { Text = "Status:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(statusCombo);

            // Add product button to allow opening product selector from edit dialog
            var addProductBtn = new Button { Content = "Thêm sản phẩm", HorizontalAlignment = HorizontalAlignment.Stretch };

            panel.Children.Add(new TextBlock { Text = "Items:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(addProductBtn);
            panel.Children.Add(new ScrollViewer { Content = itemsPanel, MaxHeight = 300 });

            panel.Children.Add(subtotalText);
            panel.Children.Add(new TextBlock { Text = "Discount:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(discountCombo);
            panel.Children.Add(totalText);

            panel.Children.Add(new TextBlock { Text = "Notes:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            panel.Children.Add(notesBox);

            // Initialize UI
            RefreshItemsUI();

            var dialog = new ContentDialog
            {
                Title = "Edit Order",
                Content = new ScrollViewer { Content = panel, MaxHeight = 700 },
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            bool openProductSelectorRequested = false;
            addProductBtn.Click += (_, _) =>
            {
                openProductSelectorRequested = true;
                try { dialog.Hide(); } catch { }
            };

            // Update totals live when discount selection changes
            discountCombo.SelectionChanged += (_, _) => RefreshItemsUI();

            while (true)
            {
                var showTask = dialog.ShowAsync();
                var result = await showTask;

                if (openProductSelectorRequested)
                {
                    openProductSelectorRequested = false;
                    var p = await ShowProductSelectorAsync();
                    if (p != null)
                    {
                        // Add to editable items or increment quantity
                        var idx = editableItems.FindIndex(ei => ei.ProductId == p.ProductId);
                        if (idx >= 0)
                        {
                            editableItems[idx].Quantity += 1;
                        }
                        else
                        {
                            editableItems.Add(new EditItem
                            {
                                ProductId = p.ProductId,
                                ProductName = p.ProductName,
                                UnitPrice = p.SellPrice,
                                Quantity = 1
                            });
                        }
                        RefreshItemsUI();
                    }
                    continue; // reopen dialog
                }

                if (result == ContentDialogResult.Primary)
                {
                    // Validate
                    if (!editableItems.Any())
                    {
                        await ShowErrorDialog("Validation Error", "Order must contain at least one item.");
                        return;
                    }

                    // compute discount as fraction 0.01..1
                    var selPct = 0;
                    if (discountCombo.SelectedItem is ComboBoxItem sc && sc.Tag is int tagPct)
                        selPct = tagPct;
                    var parsedDiscount = Math.Max(0, Math.Min(100, selPct)) / 100m;

                    var itemsDto = editableItems.Select(ei => new OrderUpsertItemDto
                    {
                        ProductId = ei.ProductId,
                        Quantity = ei.Quantity,
                        UnitPrice = ei.UnitPrice
                    }).ToList();

                    var selectedStatus = statusCombo.SelectedItem as ComboBoxItem;
                    var status = selectedStatus != null ? (OrderStatus)selectedStatus.Tag : full.Status;

                    var updateDto = new OrderUpsertRequestDto
                    {
                        CustomerId = full.CustomerId,
                        // Discount expressed as fraction 0.01..1 per requirement
                        Discount = parsedDiscount,
                        Notes = string.IsNullOrWhiteSpace(notesBox.Text) ? null : notesBox.Text.Trim(),
                        Status = status,
                        Items = itemsDto
                    };

                    // Call update and show error on failure
                    var resp = await ViewModel.UpdateOrderAsync(full.OrderId, updateDto);
                    if (resp != null && resp.Success)
                    {
                        await ViewModel.LoadOrdersCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        var msg = resp?.Message ?? "Cập nhật đơn hàng không thành công. Vui lòng thử lại.";
                        await ShowErrorDialog("Update Failed", msg);
                    }
                }

                break;
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

            var searchBox = new TextBox { PlaceholderText = "Tìm kiếm khách hàng (tên, số điện thoại)", Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8) };

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

            var searchBox = new TextBox { PlaceholderText = "Tìm kiếm sản phẩm (tên)", Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 8) };

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
