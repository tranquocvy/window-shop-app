namespace TechHaven.Infrastructure.Authorization;

/// <summary>
/// Định nghĩa các Policy name để sử dụng trong [Authorize] attribute
/// </summary>
public static class AuthorizationPolicies
{
  // Role-based policies
  public const string AdminOnly = "AdminOnly";
  public const string SellerOnly = "SellerOnly";
  public const string AdminOrSeller = "AdminOrSeller";

  // Permission-based policies
  public const string ManageProducts = "ManageProducts";
  public const string ManageOrders = "ManageOrders";
  public const string ManageCustomers = "ManageCustomers";
  public const string ManageUsers = "ManageUsers";
  public const string ViewReports = "ViewReports";
  public const string ManageSettings = "ManageSettings";
}

/// <summary>
/// Định nghĩa các Role name
/// </summary>
public static class Roles
{
  public const string Admin = "Admin";
  public const string Seller = "Seller";
}

/// <summary>
/// Định nghĩa các Permission claims
/// </summary>
public static class Permissions
{
  public const string ProductCreate = "product.create";
  public const string ProductUpdate = "product.update";
  public const string ProductDelete = "product.delete";
  public const string ProductView = "product.view";

  public const string OrderCreate = "order.create";
  public const string OrderUpdate = "order.update";
  public const string OrderDelete = "order.delete";
  public const string OrderView = "order.view";

  public const string CustomerCreate = "customer.create";
  public const string CustomerUpdate = "customer.update";
  public const string CustomerDelete = "customer.delete";
  public const string CustomerView = "customer.view";

  public const string UserCreate = "user.create";
  public const string UserUpdate = "user.update";
  public const string UserDelete = "user.delete";
  public const string UserView = "user.view";

  public const string ReportView = "report.view";
  public const string ReportExport = "report.export";

  public const string SettingCreate = "setting.create";
  public const string SettingUpdate = "setting.update";
  public const string SettingDelete = "setting.delete";
  public const string SettingView = "setting.view";
}