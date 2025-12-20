using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.ViewModel;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Views.Order
{
    /// <summary>
    /// Dialog for updating order status with validation
    /// </summary>
    internal sealed class StatusUpdateDialog
    {
        private readonly XamlRoot _xamlRoot;
        private readonly OrderViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of StatusUpdateDialog
        /// </summary>
        /// <param name="xamlRoot">XamlRoot for dialog display</param>
        /// <param name="viewModel">Order view model for API operations</param>
        public StatusUpdateDialog(XamlRoot xamlRoot, OrderViewModel viewModel)
        {
            _xamlRoot = xamlRoot ?? throw new ArgumentNullException(nameof(xamlRoot));
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        /// <summary>
        /// Shows the status update dialog for the given order
        /// </summary>
        /// <param name="orderItem">Order item to update</param>
        /// <returns>True if status was updated successfully</returns>
        public async Task<bool> ShowAsync(OrderItemViewModel orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var allowedStatuses = OrderPageHelpers.GetAllowedTransitions(orderItem.Order.Status);

            if (!allowedStatuses.Any())
            {
                await ShowErrorAsync("Không thể cập nhật",
                    $"Không thể chuyển trạng thái từ '{orderItem.StatusDisplay}' sang trạng thái khác.");
                return false;
            }

            var statusComboBox = new ComboBox
            {
                PlaceholderText = "Select new status",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 8, 0, 0)
            };

            foreach (var status in allowedStatuses)
            {
                statusComboBox.Items.Add(new ComboBoxItem
                {
                    Content = OrderPageHelpers.GetStatusDisplayName(status),
                    Tag = status
                });
            }

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = $"Current status: {orderItem.StatusDisplay}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            panel.Children.Add(statusComboBox);

            var dialog = new ContentDialog
            {
                Title = "Update Status - Order",
                Content = panel,
                PrimaryButtonText = "Update",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = _xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary || statusComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return false;

            var newStatus = (OrderStatus)selectedItem.Tag;

            var fullOrder = await _viewModel.GetOrderByIdAsync(orderItem.Order.OrderId);
            if (fullOrder == null)
            {
                await ShowErrorAsync("Load Error", "Không thể tải đơn hàng để cập nhật trạng thái.");
                return false;
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
                }).ToList() ?? new System.Collections.Generic.List<OrderUpsertItemDto>()
            };

            var response = await _viewModel.UpdateOrderAsync(orderItem.Order.OrderId, dto);
            if (response != null && response.Success)
            {
                return true;
            }

            var errorMessage = response?.Message ?? "Cập nhật trạng thái không thành công. Vui lòng thử lại hoặc kiểm tra kết nối.";
            await ShowErrorAsync("Update Failed", errorMessage);
            return false;
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
    }
}
