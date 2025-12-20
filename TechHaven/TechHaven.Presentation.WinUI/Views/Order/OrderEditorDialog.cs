using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Customers;
using TechHaven.Shared.DTOs.Orders;
using TechHaven.Shared.DTOs.Products;

namespace TechHaven.Presentation.WinUI.Views.Order
{
    /// <summary>
    /// Dialog for creating and editing orders
    /// </summary>
    internal sealed class OrderEditorDialog
    {
        #region Fields

        private readonly XamlRoot _xamlRoot;
        private readonly OrderViewModel _viewModel;
        private readonly DataTemplate? _customerItemTemplate;
        private readonly DataTemplate? _productItemTemplate;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of OrderEditorDialog
        /// </summary>
        /// <param name="xamlRoot">XamlRoot for dialog display</param>
        /// <param name="viewModel">Order view model for API operations</param>
        /// <param name="customerItemTemplate">Optional DataTemplate for customer items</param>
        /// <param name="productItemTemplate">Optional DataTemplate for product items</param>
        public OrderEditorDialog(
            XamlRoot xamlRoot,
            OrderViewModel viewModel,
            DataTemplate? customerItemTemplate = null,
            DataTemplate? productItemTemplate = null)
        {
            _xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _customerItemTemplate = customerItemTemplate;
            _productItemTemplate = productItemTemplate;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows dialog to create a new order
        /// </summary>
        public async Task ShowCreateAsync()
        {
            await ShowDialogAsync(null);
        }

        /// <summary>
        /// Shows dialog to edit an existing order
        /// </summary>
        /// <param name="orderItem">Order item to edit</param>
        public async Task ShowEditAsync(OrderItemViewModel orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var fullOrder = await _viewModel.GetOrderByIdAsync(orderItem.Order.OrderId);
            if (fullOrder == null)
            {
                await ShowErrorAsync("Load Error", "Không thể tải chi tiết đơn hàng.");
                return;
            }

            await ShowDialogAsync(fullOrder);
        }

        #endregion

        #region Private Methods - Dialog Display

        /// <summary>
        /// Shows the order editor dialog (create or edit mode)
        /// </summary>
        /// <param name="existingOrder">Existing order for edit mode, null for create mode</param>
        private async Task ShowDialogAsync(OrderDto? existingOrder)
        {
            var isEditMode = existingOrder != null;
            var state = new OrderEditorState(existingOrder);
            
            // Only allow data editing when order is Pending
            var canEditData = !isEditMode || existingOrder?.Status == OrderStatus.Pending;

            bool openCustomerSelector = false;
            bool openProductSelector = false;

            var dialogPanel = CreateDialogPanel(state, isEditMode, canEditData, out var components);

            var dialog = new ContentDialog
            {
                Title = isEditMode ? "Edit Order" : "Create New Order",
                Content = new ScrollViewer { Content = dialogPanel, MaxHeight = isEditMode ? 700 : 600 },
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

            // Only allow customer selection in create mode
            if (components.SelectCustomerButton != null)
            {
                components.SelectCustomerButton.Click += (_, _) =>
                {
                    openCustomerSelector = true;
                    try { dialog.Hide(); } catch { }
                };
            }

            // Only allow adding products when data can be edited
            if (canEditData)
            {
                components.AddProductButton.Click += (_, _) =>
                {
                    openProductSelector = true;
                    try { dialog.Hide(); } catch { }
                };
            }

            components.DiscountCombo.SelectionChanged += (_, _) => RefreshTotals(state, components);

            while (true)
            {
                var result = await dialog.ShowAsync();

                if (openCustomerSelector)
                {
                    openCustomerSelector = false;
                    await HandleCustomerSelection(state, components);
                    continue;
                }

                if (openProductSelector)
                {
                    openProductSelector = false;
                    await HandleProductSelection(state, components);
                    RefreshCartUI(state, components, canEditData);
                    continue;
                }

                if (result == ContentDialogResult.Primary)
                {
                    var success = await HandleSaveAsync(state, components, isEditMode, existingOrder, canEditData);
                    if (!success)
                        continue;
                }

                break;
            }
        }

        /// <summary>
        /// Creates the dialog panel with all UI components
        /// </summary>
        private StackPanel CreateDialogPanel(
            OrderEditorState state,
            bool isEditMode,
            bool canEditData,
            out DialogComponents components)
        {
            components = new DialogComponents();

            var panel = new StackPanel { Spacing = 8 };

            if (isEditMode && state.ExistingOrder != null)
            {
                AddOrderInfoSection(panel, state.ExistingOrder);
                AddStatusSection(panel, state, components);
                
                // Show warning if data cannot be edited
                if (!canEditData)
                {
                    panel.Children.Add(new Border
                    {
                        Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Windows.UI.Color.FromArgb(40, 255, 165, 0)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 8, 0, 0),
                        Child = new TextBlock
                        {
                            Text = "! Order details are read-only for non-Pending orders.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                Windows.UI.Color.FromArgb(255, 255, 140, 0))
                        }
                    });
                }
            }
            else
            {
                AddCustomerSection(panel, components);
            }

            AddProductsSection(panel, state, components, canEditData);
            AddDiscountSection(panel, components, state, canEditData);
            AddNotesSection(panel, components, state, canEditData);

            RefreshCartUI(state, components, canEditData);

            return panel;
        }

        /// <summary>
        /// Adds order information section (edit mode only)
        /// </summary>
        private void AddOrderInfoSection(StackPanel panel, OrderDto order)
        {
            panel.Children.Add(new TextBlock
            {
                Text = $"Customer: {order.CustomerName ?? "Walk-in Customer"}",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"]
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"User: {order.UserFullName}",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"]
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"Date: {order.OrderDate:dd/MM/yyyy HH:mm}",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TH.TextSecondary"]
            });
        }

        /// <summary>
        /// Adds status selection section (edit mode only)
        /// </summary>
        private void AddStatusSection(StackPanel panel, OrderEditorState state, DialogComponents components)
        {
            if (state.ExistingOrder == null) return;

            panel.Children.Add(new TextBlock
            {
                Text = "Status:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            components.StatusCombo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };

            var allowedStatuses = new List<OrderStatus> { state.ExistingOrder.Status };
            allowedStatuses.AddRange(OrderPageHelpers.GetAllowedTransitions(state.ExistingOrder.Status));

            foreach (var status in allowedStatuses.Distinct())
            {
                components.StatusCombo.Items.Add(new ComboBoxItem
                {
                    Content = OrderPageHelpers.GetStatusDisplayName(status),
                    Tag = status
                });
            }

            if (components.StatusCombo.Items.Count == 1)
            {
                components.StatusCombo.IsEnabled = false;
                components.StatusCombo.SelectedIndex = 0;
            }
            else
            {
                var currentItem = components.StatusCombo.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(i => (OrderStatus)i.Tag == state.ExistingOrder.Status);
                if (currentItem != null)
                    components.StatusCombo.SelectedItem = currentItem;
            }

            panel.Children.Add(components.StatusCombo);
        }

        /// <summary>
        /// Adds customer selection section (create mode only)
        /// </summary>
        private void AddCustomerSection(StackPanel panel, DialogComponents components)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "Customer:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            components.SelectCustomerButton = new Button
            {
                Content = "Chọn khách hàng (Walk-in nếu bỏ trống)",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            panel.Children.Add(components.SelectCustomerButton);
        }

        /// <summary>
        /// Adds products section
        /// </summary>
        private void AddProductsSection(StackPanel panel, OrderEditorState state, DialogComponents components, bool canEditData)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "Products:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            components.AddProductButton = new Button
            {
                Content = "Thêm sản phẩm",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = canEditData
            };

            panel.Children.Add(components.AddProductButton);

            components.CartPanel = new StackPanel { Spacing = 8 };
            panel.Children.Add(new ScrollViewer { Content = components.CartPanel, MaxHeight = 300 });

            components.SubtotalText = new TextBlock
            {
                Text = "Subtotal: 0 ₫",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };
            panel.Children.Add(components.SubtotalText);
        }

        /// <summary>
        /// Adds discount section
        /// </summary>
        private void AddDiscountSection(StackPanel panel, DialogComponents components, OrderEditorState state, bool canEditData)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "Discount:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            components.DiscountCombo = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 4, 0, 0),
                IsEnabled = canEditData
            };

            for (int p = 0; p <= 100; p++)
            {
                components.DiscountCombo.Items.Add(new ComboBoxItem { Content = $"{p}%", Tag = p });
            }
            
            // Fix discount display - the Discount field is already a fraction (0.0 - 1.0)
            if (state.ExistingOrder != null)
            {
                // Discount is stored as fraction (0.0 - 1.0), convert to percentage for display
                int discountPercent = (int)Math.Round(state.ExistingOrder.Discount * 100);
                discountPercent = Math.Max(0, Math.Min(100, discountPercent));
                components.DiscountCombo.SelectedIndex = discountPercent;
            }
            else
            {
                components.DiscountCombo.SelectedIndex = 0;
            }

            panel.Children.Add(components.DiscountCombo);

            components.TotalText = new TextBlock
            {
                Text = "Total: 0 ₫",
                FontWeight = Microsoft.UI.Text.FontWeights.Bold
            };
            panel.Children.Add(components.TotalText);
        }

        /// <summary>
        /// Adds notes section
        /// </summary>
        private void AddNotesSection(StackPanel panel, DialogComponents components, OrderEditorState state, bool canEditData)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "Notes:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            components.NotesBox = new TextBox
            {
                PlaceholderText = "Notes (optional)",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 80,
                Text = state.ExistingOrder?.Notes ?? string.Empty,
                IsReadOnly = !canEditData
            };

            panel.Children.Add(components.NotesBox);
        }

        #endregion

        #region Private Methods - Event Handlers

        /// <summary>
        /// Handles customer selection
        /// </summary>
        private async Task HandleCustomerSelection(OrderEditorState state, DialogComponents components)
        {
            var dialog = new CustomerSelectorDialog(_xamlRoot, _customerItemTemplate);
            var customer = await dialog.ShowAsync();

            state.SelectedCustomer = customer;
            if (components.SelectCustomerButton != null)
            {
                components.SelectCustomerButton.Content = customer == null
                    ? "Walk-in / None"
                    : $"{customer.CustomerName} - {customer.PhoneNumber}";
            }
        }

        /// <summary>
        /// Handles product selection and adds to cart
        /// </summary>
        private async Task HandleProductSelection(OrderEditorState state, DialogComponents components)
        {
            var dialog = new ProductSelectorDialog(_xamlRoot, _productItemTemplate);
            var product = await dialog.ShowAsync();

            if (product != null)
            {
                var existingItem = state.CartItems.FirstOrDefault(ci => ci.ProductId == product.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    state.CartItems.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        UnitPrice = product.SellPrice,
                        Quantity = 1
                    });
                }
            }
        }

        /// <summary>
        /// Handles save operation
        /// </summary>
        private async Task<bool> HandleSaveAsync(
            OrderEditorState state,
            DialogComponents components,
            bool isEditMode,
            OrderDto? existingOrder,
            bool canEditData)
        {
            if (!state.CartItems.Any())
            {
                await ShowErrorAsync("Validation Error",
                    isEditMode
                        ? "Order must contain at least one item."
                        : "Please add at least one product to the order.");
                return false;
            }

            var discountPercent = GetSelectedDiscountPercent(components.DiscountCombo);
            var discount = OrderPageHelpers.ValidateAndConvertDiscount(discountPercent);

            var dto = new OrderUpsertRequestDto
            {
                CustomerId = state.SelectedCustomer?.CustomerId ?? existingOrder?.CustomerId,
                Discount = discount,
                Notes = string.IsNullOrWhiteSpace(components.NotesBox?.Text) ? null : components.NotesBox.Text.Trim(),
                Status = GetSelectedStatus(components.StatusCombo) ?? existingOrder?.Status ?? OrderStatus.Pending,
                Items = state.CartItems.Select(ci => new OrderUpsertItemDto
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice
                }).ToList()
            };

            if (isEditMode && existingOrder != null)
            {
                // Validate status transition
                if (!ValidateStatusTransition(existingOrder.Status, dto.Status))
                {
                    await ShowErrorAsync("Invalid Transition", "Selected status change is not allowed.");
                    return false;
                }

                // If data cannot be edited but status changed, only update status
                if (!canEditData && existingOrder.Status != dto.Status)
                {
                    // Keep original data, only update status
                    dto.CustomerId = existingOrder.CustomerId;
                    dto.Discount = existingOrder.Discount;
                    dto.Notes = existingOrder.Notes;
                    dto.Items = existingOrder.Details?.Select(d => new OrderUpsertItemDto
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice
                    }).ToList() ?? new List<OrderUpsertItemDto>();
                }

                var response = await _viewModel.UpdateOrderAsync(existingOrder.OrderId, dto);
                if (response == null || !response.Success)
                {
                    var message = response?.Message ?? "Cập nhật đơn hàng không thành công. Vui lòng thử lại.";
                    await ShowErrorAsync("Update Failed", message);
                    return false;
                }
            }
            else
            {
                await _viewModel.CreateOrderCommand.ExecuteAsync(dto);
            }

            await _viewModel.LoadOrdersCommand.ExecuteAsync(null);
            return true;
        }

        #endregion

        #region Private Methods - UI Refresh

        /// <summary>
        /// Refreshes the cart UI
        /// </summary>
        private void RefreshCartUI(OrderEditorState state, DialogComponents components, bool canEditData)
        {
            components.CartPanel.Children.Clear();

            foreach (var item in state.CartItems)
            {
                var grid = CreateCartItemGrid(item, state, components, canEditData);
                components.CartPanel.Children.Add(grid);
            }

            RefreshTotals(state, components);
        }

        /// <summary>
        /// Creates a grid for displaying a cart item
        /// </summary>
        private Grid CreateCartItemGrid(CartItem item, OrderEditorState state, DialogComponents components, bool canEditData)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(80) },
                    new ColumnDefinition { Width = new GridLength(100) },
                    new ColumnDefinition { Width = new GridLength(40) }
                },
                Margin = new Thickness(0, 4, 0, 4)
            };

            var nameBlock = new TextBlock
            {
                Text = item.ProductName,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };
            Grid.SetColumn(nameBlock, 0);

            var qtyBox = new TextBox
            {
                Text = item.Quantity.ToString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 60,
                IsReadOnly = !canEditData
            };
            Grid.SetColumn(qtyBox, 1);

            var priceText = new TextBlock
            {
                Text = item.UnitPrice.ToString("N0") + " ₫",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(priceText, 2);

            var removeBtn = new Button
            {
                Content = "X",
                Width = 36,
                HorizontalAlignment = HorizontalAlignment.Right,
                IsEnabled = canEditData
            };
            Grid.SetColumn(removeBtn, 3);

            // Only allow editing if data can be edited
            if (canEditData)
            {
                qtyBox.TextChanged += (_, _) =>
                {
                    if (int.TryParse(qtyBox.Text, out var q) && q > 0)
                    {
                        item.Quantity = q;
                        RefreshTotals(state, components);
                    }
                };

                removeBtn.Click += (_, _) =>
                {
                    state.CartItems.Remove(item);
                    RefreshCartUI(state, components, canEditData);
                };
            }

            grid.Children.Add(nameBlock);
            grid.Children.Add(qtyBox);
            grid.Children.Add(priceText);
            grid.Children.Add(removeBtn);

            return grid;
        }

        /// <summary>
        /// Refreshes total calculations
        /// </summary>
        private void RefreshTotals(OrderEditorState state, DialogComponents components)
        {
            var subtotal = state.CartItems.Sum(item => item.Quantity * item.UnitPrice);
            components.SubtotalText.Text = $"Subtotal: {subtotal:N0} ₫";

            var discountPercent = GetSelectedDiscountPercent(components.DiscountCombo);
            var discountFraction = OrderPageHelpers.ValidateAndConvertDiscount(discountPercent);
            var total = Math.Max(0, subtotal - (subtotal * discountFraction));

            components.TotalText.Text = $"Total: {total:N0} ₫";
        }

        #endregion

        #region Private Methods - Helpers

        /// <summary>
        /// Gets selected discount percentage from combo box
        /// </summary>
        private int GetSelectedDiscountPercent(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag is int percent)
                return percent;
            return 0;
        }

        /// <summary>
        /// Gets selected status from combo box
        /// </summary>
        private OrderStatus? GetSelectedStatus(ComboBox? comboBox)
        {
            if (comboBox?.SelectedItem is ComboBoxItem item && item.Tag is OrderStatus status)
                return status;
            return null;
        }

        /// <summary>
        /// Validates status transition
        /// </summary>
        private bool ValidateStatusTransition(OrderStatus current, OrderStatus target)
        {
            return OrderPageHelpers.IsValidStatusTransition(current, target);
        }

        /// <summary>
        /// Shows an error dialog
        /// </summary>
        private async Task ShowErrorAsync(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = _xamlRoot
            };
            await dialog.ShowAsync();
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Represents a cart item in the order
        /// </summary>
        private class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }

        /// <summary>
        /// Maintains state for order editor dialog
        /// </summary>
        private class OrderEditorState
        {
            public OrderDto? ExistingOrder { get; }
            public CustomerDto? SelectedCustomer { get; set; }
            public List<CartItem> CartItems { get; }

            public OrderEditorState(OrderDto? existingOrder)
            {
                ExistingOrder = existingOrder;
                CartItems = existingOrder?.Details?.Select(d => new CartItem
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName ?? "Unknown",
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity
                }).ToList() ?? new List<CartItem>();
            }
        }

        /// <summary>
        /// Contains references to dialog UI components
        /// </summary>
        private class DialogComponents
        {
            public Button? SelectCustomerButton { get; set; }
            public Button AddProductButton { get; set; } = null!;
            public StackPanel CartPanel { get; set; } = null!;
            public TextBlock SubtotalText { get; set; } = null!;
            public ComboBox DiscountCombo { get; set; } = null!;
            public TextBlock TotalText { get; set; } = null!;
            public TextBox? NotesBox { get; set; }
            public ComboBox? StatusCombo { get; set; }
        }

        #endregion
    }
}
