using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities
{
    /// <summary>
    /// Represents a customer in the system.
    /// </summary>
    [Table("Customers")]
    public class Customer
    {
        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the customer.
        /// </summary>
        [Required(ErrorMessage = "Customer name is required.")]
        [MaxLength(150, ErrorMessage = "Customer name cannot exceed 150 characters.")]
        [Display(Name = "Customer Name")]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the customer.
        /// </summary>
        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^[\d\-\+\(\)\s]+$", ErrorMessage = "Phone number contains invalid characters.")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address of the customer.
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(150, ErrorMessage = "Email address cannot exceed 150 characters.")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the address of the customer.
        /// </summary>
        [MaxLength(300, ErrorMessage = "Address cannot exceed 300 characters.")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        /// <summary>
        /// Gets or sets the type of customer (Regular, VIP, or Student).
        /// </summary>
        [Required(ErrorMessage = "Customer type is required.")]
        [Display(Name = "Customer Type")]
        public CustomerType Type { get; set; } = CustomerType.Regular;

        /// <summary>
        /// Gets or sets the total amount purchased by the customer.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Purchased")]
        [Range(0, double.MaxValue, ErrorMessage = "Total purchased cannot be negative.")]
        public decimal TotalPurchased { get; set; } = 0;

        /// <summary>
        /// Gets or sets the date and time when the customer record was created.
        /// </summary>
        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the collection of orders associated with this customer.
        /// </summary>
        public ICollection<Order>? Orders { get; set; }
    }

    /// <summary>
    /// Defines the types of customers in the system.
    /// </summary>
    public enum CustomerType
    {
        /// <summary>
        /// Regular customer type.
        /// </summary>
        Regular = 1,

        /// <summary>
        /// VIP customer type with special privileges.
        /// </summary>
        VIP = 2,

        /// <summary>
        /// Student customer type with discounts.
        /// </summary>
        Student = 3
    }
}
