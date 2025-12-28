namespace TechHaven.Shared.DTOs.Payments;

public class PaymentCreateDto
{
    public int OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }
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
    CreditCard = 3
}