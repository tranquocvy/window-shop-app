namespace TechHaven.Domain.Enums;

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