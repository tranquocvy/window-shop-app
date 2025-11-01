using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities
{
    /// <summary>
    /// Represents an order in the system.
    /// </summary>
    [Table("Orders")]
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the customer who placed the order. Null if it's a walk-in sale.
        /// </summary>
        [Display(Name = "Customer")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user (employee) who created the order.
        /// </summary>
        [Required(ErrorMessage = "User is required.")]
        [Display(Name = "Created By")]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was placed.
        /// </summary>
        [Required(ErrorMessage = "Order date is required.")]
        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the current status of the order.
        /// </summary>
        [Required(ErrorMessage = "Order status is required.")]
        [Display(Name = "Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Gets or sets the subtotal amount before discount and tax.
        /// </summary>
        [Required(ErrorMessage = "Subtotal is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        [Range(0, double.MaxValue, ErrorMessage = "Subtotal cannot be negative.")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Gets or sets the discount amount applied to the order.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative.")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the total amount after discount and tax.
        /// </summary>
        [Required(ErrorMessage = "Total amount is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets additional notes or comments for the order.
        /// </summary>
        [Display(Name = "Notes")]
        [Column(TypeName = "nvarchar(max)")]
        [DataType(DataType.MultilineText)]
        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the customer navigation property.
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// Gets or sets the user (employee) navigation property who created the order.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Gets or sets the collection of order details for this order.
        /// </summary>
        public ICollection<OrderDetail>? OrderDetails { get; set; }

        /// <summary>
        /// Gets or sets the collection of payments made for this order.
        /// </summary>
        public ICollection<Payment>? Payments { get; set; }
    }

    /// <summary>
    /// Defines the possible statuses of an order.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order is pending and awaiting processing.
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Order is currently being processed.
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Order has been completed successfully.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 4,

        /// <summary>
        /// Order has been returned.
        /// </summary>
        Returned = 5
    }

    /// <summary>
    /// Defines the available payment methods.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Cash payment method.
        /// </summary>
        Cash = 1,

        /// <summary>
        /// Bank transfer payment method.
        /// </summary>
        BankTransfer = 2,

        /// <summary>
        /// Credit card payment method.
        /// </summary>
        CreditCard = 3,

        /// <summary>
        /// Installment payment method.
        /// </summary>
        Installment = 4
    }
}

