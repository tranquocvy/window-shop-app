using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities;

/// <summary>
/// Represents a role in the system that defines user permissions and access levels.
/// </summary>
[Table("Roles")]
public class Role
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    [Required(ErrorMessage = "Role name is required.")]
    [MaxLength(100, ErrorMessage = "Role name cannot exceed 100 characters.")]
    [Display(Name = "Role Name")]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role and its permissions.
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of users assigned to this role.
    /// </summary>
    public ICollection<User>? Users { get; set; }
}
