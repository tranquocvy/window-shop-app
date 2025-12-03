using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Mock;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Orders;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Net.Http;
using TechHaven.Presentation.WinUI.Services.Http;

namespace TechHaven.Presentation.WinUI.ViewModel
{
    public partial class OrderViewModel : ObservableObject
    {
        private static readonly HttpClient SharedHttpClient = ApiClientFactory.GetHttpClient();
        private readonly IOrderService _orderService = new HttpOrderService(SharedHttpClient);
        private readonly IOrderPdfService _orderPdfService;

        public ObservableCollection<OrderItemViewModel> Orders { get; } = new();

        public IAsyncRelayCommand<OrderItemViewModel> PrintOrderCommand { get; }

        // Observable properties
        [ObservableProperty]
        private string? _searchKeyword;

        [ObservableProperty]
        private DateTimeOffset? _fromDate = DateTimeOffset.Now;

        [ObservableProperty]
        private DateTimeOffset? _toDate = DateTimeOffset.Now;

        [ObservableProperty]
        private OrderStatusItem? _selectedStatusItem;

        [ObservableProperty]
        private int _pageNumber = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _totalPages;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private bool _canGoNext;

        [ObservableProperty]
        private bool _canGoPrevious;

        [ObservableProperty]
        private string _pageInfo = "Page 1";

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<OrderStatusItem> StatusList { get; } = new()
        {
            new OrderStatusItem { DisplayName = "All", Status = null },
            new OrderStatusItem { DisplayName = "Pending", Status = OrderStatus.Pending },
            new OrderStatusItem { DisplayName = "Processing", Status = OrderStatus.Processing },
            new OrderStatusItem { DisplayName = "Completed", Status = OrderStatus.Completed },
            new OrderStatusItem { DisplayName = "Cancelled", Status = OrderStatus.Cancelled },
            new OrderStatusItem { DisplayName = "Returned", Status = OrderStatus.Returned }
        };

        // Parameterless ctor chains to default OrderPdfService
        public OrderViewModel() : this(new OrderPdfService()) { }

        public OrderViewModel(IOrderPdfService orderPdfService)
        {
            _orderPdfService = orderPdfService; // ensure field initialized

            PrintOrderCommand = new AsyncRelayCommand<OrderItemViewModel>(PrintOrderAsync);

            // Set default selected status to "All"
            SelectedStatusItem = StatusList[0];

            // Load initial data
            _ = LoadOrdersAsync();
        }

        private async Task PrintOrderAsync(OrderItemViewModel item)
        {
            if (item == null) return;

            try
            {
                var resp = await _orderService.GetOrderByIdAsync(item.Order.OrderId);
                if (!resp.Success || resp.Data == null)
                {
                    await ShowDialogAsync("Export Error", "Failed to load order details.");
                    return;
                }

                var bytes = await _orderPdfService.GenerateOrderPdfAsync(resp.Data);
                if (bytes == null || bytes.Length == 0)
                {
                    await ShowDialogAsync("Export Error", "PDF generator returned no data.");
                    return;
                }

                var fileName = $"Order_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                var path = await FileSaveHelper.SavePdfToDownloadsAsync(bytes, fileName);
                FileSaveHelper.OpenFile(path);

                await ShowDialogAsync("Exported", $"PDF saved to: {path}");
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Export Error", ex.ToString());
            }
        }

        private async Task ShowDialogAsync(string title, string message)
        {
            var dlg = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Close",
                XamlRoot = App.MainWindow.Content.XamlRoot
            };
            await dlg.ShowAsync();
        }

        // Auto-reload when search keyword changes
        partial void OnSearchKeywordChanged(string? value)
        {
            PageNumber = 1;
            _ = LoadOrdersAsync();
        }

        // Auto-reload when from date changes
        partial void OnFromDateChanged(DateTimeOffset? value)
        {
            PageNumber = 1;
            _ = LoadOrdersAsync();
        }

        // Auto-reload when to date changes
        partial void OnToDateChanged(DateTimeOffset? value)
        {
            PageNumber = 1;
            _ = LoadOrdersAsync();
        }

        // Reload when page size changes
        partial void OnPageSizeChanged(int value)
        {
            PageNumber = 1;
            _ = LoadOrdersAsync();
        }

        [RelayCommand]
        private async Task LoadOrdersAsync()
        {
            IsLoading = true;
            Orders.Clear();

            try
            {
                var query = new OrderListQueryDto
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    CustomerKeyword = SearchKeyword,
                    Status = SelectedStatusItem?.Status,
                    OrderDate = (FromDate.HasValue || ToDate.HasValue)
                        ? new DateRangeFilter
                        {
                            StartDate = FromDate?.DateTime,
                            EndDate = ToDate?.DateTime
                        }
                        : null,
                    Sorting = new SortingOption { SortBy = "date", Desc = true }
                };

                var response = await _orderService.GetOrdersAsync(query);

                if (response.Success && response.Data != null)
                {
                    foreach (var order in response.Data.Items)
                    {
                        Orders.Add(new OrderItemViewModel(order));
                    }

                    TotalCount = response.Data.TotalCount;
                    TotalPages = response.Data.TotalPages;
                    CanGoPrevious = PageNumber > 1;
                    CanGoNext = PageNumber < TotalPages;
                    PageInfo = $"Page {PageNumber} / {TotalPages} (Total: {TotalCount} orders)";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            PageNumber = 1;
            await LoadOrdersAsync();
        }

        [RelayCommand]
        private async Task ClearFilterAsync()
        {
            SearchKeyword = null;
            FromDate = null;
            ToDate = null;
            SelectedStatusItem = StatusList[0]; // Reset to "All"
            PageNumber = 1;
            await LoadOrdersAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (PageNumber > 1)
            {
                PageNumber--;
                await LoadOrdersAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (PageNumber < TotalPages)
            {
                PageNumber++;
                await LoadOrdersAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteOrderAsync(OrderItemViewModel item)
        {
            if (item == null) return;

            var response = await _orderService.DeleteOrderAsync(item.Order.OrderId);
            if (response.Success)
            {
                Orders.Remove(item);
                TotalCount--;
                
                // Reload current page if empty
                if (Orders.Count == 0 && PageNumber > 1)
                {
                    PageNumber--;
                    await LoadOrdersAsync();
                }
                else
                {
                    PageInfo = $"Page {PageNumber} / {TotalPages} (Total: {TotalCount} orders)";
                }
            }
        }

        public async Task<ResponseWrapper<OrderDto>?> UpdateOrderAsync(int orderId, OrderUpsertRequestDto dto)
        {
            if (dto == null) return null;

            try
            {
                var response = await _orderService.UpdateOrderAsync(orderId, dto);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var resp = await _orderService.GetOrderByIdAsync(orderId);
                if (resp != null && resp.Success && resp.Data != null)
                    return resp.Data;
            }
            catch
            {
                // ignore
            }
            return null;
        }

        [RelayCommand]
        private void ViewOrderDetail(OrderItemViewModel item)
        {
            if (item == null) return;
            // TODO: Navigate to order detail page
        }

        [RelayCommand]
        private async Task CreateOrderAsync(OrderUpsertRequestDto dto)
        {
            if (dto == null) return;

            try
            {
                var response = await _orderService.CreateOrderAsync(dto);
                if (response.Success)
                {
                    // Order created successfully
                    // Reload will be called from the UI
                }
            }
            catch (Exception)
            {
                // Handle error silently for now
            }
        }
    }

    public partial class OrderItemViewModel : ObservableObject
    {
        public OrderDto Order { get; }

        public OrderItemViewModel(OrderDto order)
        {
            Order = order;
        }

        public string StatusDisplay => Order.Status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Processing => "Processing",
            OrderStatus.Completed => "Completed",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Returned => "Returned",
            _ => "Unknown"
        };

        public Color StatusColor => Order.Status switch
        {
            OrderStatus.Pending => Color.FromArgb(255, 255, 165, 0),      // Orange
            OrderStatus.Processing => Color.FromArgb(255, 33, 150, 243),   // Blue
            OrderStatus.Completed => Color.FromArgb(255, 76, 175, 80),     // Green
            OrderStatus.Cancelled => Color.FromArgb(255, 244, 67, 54),     // Red
            OrderStatus.Returned => Color.FromArgb(255, 158, 158, 158),    // Gray
            _ => Color.FromArgb(255, 0, 0, 0)
        };

        public string CustomerDisplay => Order.CustomerName ?? "Walk-in Customer";

        public string OrderDateDisplay => Order.OrderDate.ToString("dd/MM/yyyy HH:mm");

        public string TotalAmountDisplay => Order.TotalAmount.ToString("N0") + " ₫";

        public string ProductsCountDisplay
        {
            get
            {
                var count = Order.Details?.Count ?? 0;
                var totalQty = Order.Details?.Sum(d => d.Quantity) ?? 0;
                return $"{count} items ({totalQty} qty)";
            }
        }
    }

    public class OrderStatusItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public OrderStatus? Status { get; set; }
    }
}
