using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities;

/// <summary>
/// Represents a payment transaction for an order.
/// </summary>
[Table("Payments")]
public class Payment
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the order this payment belongs to.
    /// </summary>
    [Required(ErrorMessage = "Order is required.")]
    [Display(Name = "Order")]
    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the payment method used (e.g., Cash, Card, BankTransfer).
    /// </summary>
    [Required(ErrorMessage = "Payment method is required.")]
    [MaxLength(50, ErrorMessage = "Payment method cannot exceed 50 characters.")]
    [Display(Name = "Payment Method")]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount paid.
    /// </summary>
    [Required(ErrorMessage = "Payment amount is required.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Amount")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was made.
    /// </summary>
    [Required(ErrorMessage = "Payment date is required.")]
    [Display(Name = "Payment Date")]
    [DataType(DataType.DateTime)]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the order navigation property.
    /// </summary>
    public Order? Order { get; set; }
}
