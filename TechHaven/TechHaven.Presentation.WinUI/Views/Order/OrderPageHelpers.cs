using System;
using TechHaven.Shared.DTOs.Orders;

namespace TechHaven.Presentation.WinUI.Views.Order
{
    /// <summary>
    /// Helper methods and utilities for order page operations
    /// </summary>
    internal static class OrderPageHelpers
    {
        /// <summary>
        /// Gets the display name for an order status
        /// </summary>
        /// <param name="status">Order status enum value</param>
        /// <returns>Localized display name</returns>
        public static string GetStatusDisplayName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.Processing => "Processing",
                OrderStatus.Completed => "Completed",
                OrderStatus.Cancelled => "Cancelled",
                OrderStatus.Returned => "Returned",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Validates if status transition is allowed
        /// </summary>
        /// <param name="currentStatus">Current order status</param>
        /// <param name="newStatus">Target status to transition to</param>
        /// <returns>True if transition is valid</returns>
        public static bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => newStatus is OrderStatus.Pending or OrderStatus.Processing or OrderStatus.Cancelled,
                OrderStatus.Processing => newStatus is OrderStatus.Processing or OrderStatus.Completed or OrderStatus.Cancelled,
                OrderStatus.Completed => newStatus is OrderStatus.Completed or OrderStatus.Returned,
                OrderStatus.Cancelled => newStatus == OrderStatus.Cancelled,
                OrderStatus.Returned => newStatus == OrderStatus.Returned,
                _ => newStatus == currentStatus
            };
        }

        /// <summary>
        /// Gets the list of allowed status transitions from current status
        /// </summary>
        /// <param name="currentStatus">Current order status</param>
        /// <returns>Array of allowed target statuses</returns>
        public static OrderStatus[] GetAllowedTransitions(OrderStatus currentStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
                OrderStatus.Processing => new[] { OrderStatus.Completed, OrderStatus.Cancelled },
                OrderStatus.Completed => new[] { OrderStatus.Returned },
                _ => Array.Empty<OrderStatus>()
            };
        }

        /// <summary>
        /// Validates discount percentage
        /// </summary>
        /// <param name="discountPercent">Discount value as percentage (0-100)</param>
        /// <returns>Validated discount as fraction (0.0-1.0)</returns>
        public static decimal ValidateAndConvertDiscount(int discountPercent)
        {
            return Math.Max(0, Math.Min(100, discountPercent)) / 100m;
        }
    }
}
