using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities;

/// <summary>
/// Represents a user (employee) in the system.
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
    [Display(Name = "Full Name")]
    [StringLength(150)]
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username used for login.
    /// </summary>
    [Required(ErrorMessage = "Username is required.")]
    [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [Display(Name = "Username")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [MaxLength(256)]
    [Display(Name = "Password Hash")]
    [DataType(DataType.Password)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the role assigned to this user.
    /// </summary>
    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the user has seen the guide/tutorial.
    /// </summary>
    [Display(Name = "Has Seen Guide")]
    public bool HasSeenGuide { get; set; } = false;

    /// <summary>
    /// Gets or sets the role navigation property.
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Gets or sets the collection of orders created by this user.
    /// </summary>
    public ICollection<Order>? Orders { get; set; }

    /// <summary>
    /// Gets or sets the collection of commissions earned by this user.
    /// </summary>
    public ICollection<Commission>? Commissions { get; set; }
}