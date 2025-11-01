using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities;

/// <summary>
/// Represents a line item detail in an order, containing product and quantity information.
/// </summary>
[Table("OrderDetails")]
public class OrderDetail
{
    /// <summary>
    /// Gets or sets the unique identifier for the order detail.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderDetailId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the order this detail belongs to.
    /// </summary>
    [Required(ErrorMessage = "Order is required.")]
    [Display(Name = "Order")]
    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the product in this order detail.
    /// </summary>
    [Required(ErrorMessage = "Product is required.")]
    [Display(Name = "Product")]
    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product ordered.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required.")]
    [Display(Name = "Quantity")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product at the time of order.
    /// </summary>
    [Required(ErrorMessage = "Unit price is required.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Unit Price")]
    [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the calculated subtotal for this order detail (Quantity × UnitPrice).
    /// </summary>
    [NotMapped]
    [Display(Name = "Subtotal")]
    public decimal SubTotal => Quantity * UnitPrice;

    /// <summary>
    /// Gets or sets the order navigation property.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Gets or sets the product navigation property.
    /// </summary>
    public Product? Product { get; set; }
}
