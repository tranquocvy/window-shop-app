namespace TechHaven.Domain.Enums;

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
    /// Student customer type with discounts.
    /// </summary>
    Student = 2,

    /// <summary>
    /// VIP customer type with special privileges.
    /// </summary>
    VIP = 3
}