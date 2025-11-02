using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities;

/// <summary>
/// Represents a commission record for a user based on their sales performance.
/// </summary>
[Table("Commissions")]
public class Commission
{
    /// <summary>
    /// Gets or sets the unique identifier for the commission record.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CommissionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who earned this commission.
    /// </summary>
    [Required(ErrorMessage = "User is required.")]
    [Display(Name = "User")]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the month for which the commission is calculated (1-12).
    /// </summary>
    [Required(ErrorMessage = "Month is required.")]
    [Display(Name = "Month")]
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    /// <summary>
    /// Gets or sets the year for which the commission is calculated.
    /// </summary>
    [Required(ErrorMessage = "Year is required.")]
    [Display(Name = "Year")]
    [Range(2000, 9999, ErrorMessage = "Year must be a valid 4-digit year.")]
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the total sales amount for the period.
    /// </summary>
    [Required(ErrorMessage = "Total sales is required.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total Sales")]
    [Range(0, double.MaxValue, ErrorMessage = "Total sales cannot be negative.")]
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Gets or sets the commission rate percentage (e.g., 5.5 for 5.5%).
    /// </summary>
    [Required(ErrorMessage = "Commission rate is required.")]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Commission Rate (%)")]
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100.")]
    public decimal CommissionRate { get; set; }

    /// <summary>
    /// Gets or sets the calculated commission amount based on total sales and rate.
    /// </summary>
    [Required(ErrorMessage = "Commission amount is required.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Commission Amount")]
    [Range(0, double.MaxValue, ErrorMessage = "Commission amount cannot be negative.")]
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// Gets or sets the user navigation property.
    /// </summary>
    public User? User { get; set; }
}
