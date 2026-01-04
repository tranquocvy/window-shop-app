namespace TechHaven.Domain.Enums;

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