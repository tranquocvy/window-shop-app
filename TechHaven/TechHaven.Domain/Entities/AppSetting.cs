using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Uncomment after installing EF Core
// using Microsoft.EntityFrameworkCore;

namespace TechHaven.Domain.Entities
{
    /// <summary>
    /// Represents an application setting that can be configured and stored in the database.
    /// </summary>
    [Table("AppSettings")]
    public class AppSetting
    {
        /// <summary>
        /// Gets or sets the unique identifier for the application setting.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppSettingId { get; set; }

        /// <summary>
        /// Gets or sets the unique key for the setting (e.g., "Store.Name", "POS.AutoPrint").
        /// </summary>
        [Required(ErrorMessage = "Setting key is required.")]
        [MaxLength(100, ErrorMessage = "Key cannot exceed 100 characters.")]
        [Display(Name = "Key")]
        [RegularExpression(@"^[A-Za-z0-9._]+$", ErrorMessage = "Key can only contain letters, numbers, dots, and underscores.")]
        // [Comment("Unique key for the configuration, e.g., Store.Name, POS.AutoPrint")]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the setting stored as a string, to be parsed according to ValueType.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Value cannot exceed 1000 characters.")]
        [Display(Name = "Value")]
        // [Comment("Configuration value stored as string, parsed according to ValueType")]
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the data type of the value (String, Number, Decimal, Bool, Json).
        /// </summary>
        [Required(ErrorMessage = "Value type is required.")]
        [Display(Name = "Value Type")]
        // [Comment("Actual data type of the Value (String, Number, Decimal, Bool, Json)")]
        public SettingType ValueType { get; set; } = SettingType.String;

        /// <summary>
        /// Gets or sets the category/group of the setting (e.g., "Store", "Invoice", "POS", "Payment").
        /// </summary>
        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        [Display(Name = "Category")]
        // [Comment("Setting category/group, e.g., Store, Invoice, POS, Payment")]
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets a brief description of the setting's purpose.
        /// </summary>
        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        // [Comment("Brief description of the setting's purpose")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system setting that cannot be deleted.
        /// </summary>
        [Required(ErrorMessage = "IsSystem flag is required.")]
        [Display(Name = "Is System Setting")]
        // [Comment("Marks system configuration that cannot be deleted if true")]
        public bool IsSystem { get; set; } = false;

        /// <summary>
        /// Gets or sets the date and time when the setting was last updated.
        /// </summary>
        [Required(ErrorMessage = "Updated date is required.")]
        [Display(Name = "Updated At")]
        [DataType(DataType.DateTime)]
        // [Comment("Timestamp of the last update to this configuration")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Defines the data types available for application settings.
    /// </summary>
    public enum SettingType
    {
        /// <summary>
        /// String value type.
        /// </summary>
        String = 0,

        /// <summary>
        /// Integer number value type.
        /// </summary>
        Number = 1,

        /// <summary>
        /// Decimal number value type.
        /// </summary>
        Decimal = 2,

        /// <summary>
        /// Boolean value type.
        /// </summary>
        Bool = 3,

        /// <summary>
        /// JSON object value type.
        /// </summary>
        Json = 4
    }

    /// <summary>
    /// Provides constant keys for commonly used application settings.
    /// </summary>
    public static class SettingKeys
    {
        /// <summary>
        /// Key for store name setting.
        /// </summary>
        public const string StoreName = "Store.Name";

        /// <summary>
        /// Key for store phone number setting.
        /// </summary>
        public const string StorePhone = "Store.Phone";

        /// <summary>
        /// Key for store address setting.
        /// </summary>
        public const string StoreAddress = "Store.Address";

        /// <summary>
        /// Key for invoice footer text setting.
        /// </summary>
        public const string InvoiceFooter = "Invoice.Footer";

        /// <summary>
        /// Key for automatic receipt printing setting.
        /// </summary>
        public const string AutoPrintReceipt = "POS.AutoPrint";

        /// <summary>
        /// Key for default currency setting.
        /// </summary>
        public const string DefaultCurrency = "Currency.Default";
    }
}
