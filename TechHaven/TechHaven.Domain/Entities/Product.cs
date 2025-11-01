using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechHaven.Domain.Entities
{
    /// <summary>
    /// Represents a product in the inventory.
    /// </summary>
    [Table("Products")]
    public class Product
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the category this product belongs to.
        /// </summary>
        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the brand name of the product.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Brand name cannot exceed 100 characters.")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the color of the product.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Color cannot exceed 50 characters.")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        /// <summary>
        /// Gets or sets the storage capacity in GB.
        /// </summary>
        [Display(Name = "Storage Capacity (GB)")]
        [Range(0, int.MaxValue, ErrorMessage = "Storage capacity must be a positive number.")]
        public int? StorageCapacity { get; set; }

        /// <summary>
        /// Gets or sets the processor information of the product.
        /// </summary>
        [MaxLength(100, ErrorMessage = "Processor cannot exceed 100 characters.")]
        [Display(Name = "Processor")]
        public string? Processor { get; set; }

        /// <summary>
        /// Gets or sets the screen size in inches.
        /// </summary>
        [Display(Name = "Screen Size (inches)")]
        [Range(0, double.MaxValue, ErrorMessage = "Screen size must be a positive number.")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ScreenSize { get; set; }

        /// <summary>
        /// Gets or sets the battery capacity in mAh.
        /// </summary>
        [Display(Name = "Battery Capacity (mAh)")]
        [Range(0, int.MaxValue, ErrorMessage = "Battery capacity must be a positive number.")]
        public int? BatteryCapacity { get; set; }

        /// <summary>
        /// Gets or sets the URL of the main product image.
        /// </summary>
        [MaxLength(300, ErrorMessage = "Image URL cannot exceed 300 characters.")]
        [Display(Name = "Image URL")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the JSON string containing a gallery of product images.
        /// </summary>
        [Display(Name = "Image Gallery (JSON)")]
        [Column(TypeName = "nvarchar(max)")]
        public string? ImageGalleryJson { get; set; }

        /// <summary>
        /// Gets or sets the cost price of the product.
        /// </summary>
        [Required(ErrorMessage = "Cost price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost price cannot be negative.")]
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Gets or sets the selling price of the product.
        /// </summary>
        [Required(ErrorMessage = "Sell price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Sell Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Sell price cannot be negative.")]
        public decimal SellPrice { get; set; }

        /// <summary>
        /// Gets or sets the current stock quantity of the product.
        /// </summary>
        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// Gets or sets the description of the product.
        /// </summary>
        [Display(Name = "Description")]
        [Column(TypeName = "nvarchar(max)")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is in draft status.
        /// </summary>
        [Display(Name = "Is Draft")]
        public bool IsDraft { get; set; } = false;

        /// <summary>
        /// Gets or sets the date and time when the product was created.
        /// </summary>
        [Display(Name = "Created At")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the date and time when the product was last updated.
        /// </summary>
        [Display(Name = "Updated At")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the category navigation property.
        /// </summary>
        public Category? Category { get; set; }

        /// <summary>
        /// Gets or sets the collection of order details that include this product.
        /// </summary>
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
