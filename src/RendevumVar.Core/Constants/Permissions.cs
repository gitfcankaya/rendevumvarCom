namespace RendevumVar.Core.Constants;

/// <summary>
/// Application-wide permission constants for role-based access control
/// </summary>
public static class Permissions
{
    // Staff Management Permissions
    public const string ViewStaff = "Permissions.Staff.View";
    public const string CreateStaff = "Permissions.Staff.Create";
    public const string EditStaff = "Permissions.Staff.Edit";
    public const string DeleteStaff = "Permissions.Staff.Delete";
    public const string ManageStaff = "Permissions.Staff.Manage";
    public const string InviteStaff = "Permissions.Staff.Invite";

    // Schedule Management Permissions
    public const string ViewSchedules = "Permissions.Schedules.View";
    public const string CreateSchedules = "Permissions.Schedules.Create";
    public const string EditSchedules = "Permissions.Schedules.Edit";
    public const string DeleteSchedules = "Permissions.Schedules.Delete";
    public const string ManageSchedules = "Permissions.Schedules.Manage";

    // Time Off Permissions
    public const string ViewTimeOff = "Permissions.TimeOff.View";
    public const string RequestTimeOff = "Permissions.TimeOff.Request";
    public const string ApproveTimeOff = "Permissions.TimeOff.Approve";
    public const string RejectTimeOff = "Permissions.TimeOff.Reject";
    public const string ManageTimeOff = "Permissions.TimeOff.Manage";

    // Role Management Permissions
    public const string ViewRoles = "Permissions.Roles.View";
    public const string CreateRoles = "Permissions.Roles.Create";
    public const string EditRoles = "Permissions.Roles.Edit";
    public const string DeleteRoles = "Permissions.Roles.Delete";
    public const string ManageRoles = "Permissions.Roles.Manage";

    // Appointment Permissions
    public const string ViewAppointments = "Permissions.Appointments.View";
    public const string CreateAppointments = "Permissions.Appointments.Create";
    public const string EditAppointments = "Permissions.Appointments.Edit";
    public const string DeleteAppointments = "Permissions.Appointments.Delete";
    public const string ManageAppointments = "Permissions.Appointments.Manage";

    // Customer Permissions
    public const string ViewCustomers = "Permissions.Customers.View";
    public const string CreateCustomers = "Permissions.Customers.Create";
    public const string EditCustomers = "Permissions.Customers.Edit";
    public const string DeleteCustomers = "Permissions.Customers.Delete";
    public const string ManageCustomers = "Permissions.Customers.Manage";

    // Service Management Permissions
    public const string ViewServices = "Permissions.Services.View";
    public const string CreateServices = "Permissions.Services.Create";
    public const string EditServices = "Permissions.Services.Edit";
    public const string DeleteServices = "Permissions.Services.Delete";
    public const string ManageServices = "Permissions.Services.Manage";

    // Salon Management Permissions
    public const string ViewSalons = "Permissions.Salons.View";
    public const string CreateSalons = "Permissions.Salons.Create";
    public const string EditSalons = "Permissions.Salons.Edit";
    public const string DeleteSalons = "Permissions.Salons.Delete";
    public const string ManageSalons = "Permissions.Salons.Manage";

    // Reports and Analytics Permissions
    public const string ViewReports = "Permissions.Reports.View";
    public const string ExportReports = "Permissions.Reports.Export";
    public const string ManageReports = "Permissions.Reports.Manage";

    // Settings Permissions
    public const string ViewSettings = "Permissions.Settings.View";
    public const string EditSettings = "Permissions.Settings.Edit";
    public const string ManageSettings = "Permissions.Settings.Manage";

    // Billing and Subscription Permissions
    public const string ViewBilling = "Permissions.Billing.View";
    public const string ManageBilling = "Permissions.Billing.Manage";
    public const string ViewSubscription = "Permissions.Subscription.View";
    public const string ManageSubscription = "Permissions.Subscription.Manage";

    // System Admin Permissions
    public const string SystemAdmin = "Permissions.System.Admin";
    public const string ManageAllTenants = "Permissions.System.ManageAllTenants";

    /// <summary>
    /// Get all available permissions as a list
    /// </summary>
    public static List<string> GetAllPermissions()
    {
        return typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => (string)fi.GetValue(null)!)
            .ToList();
    }

    /// <summary>
    /// Get permissions by category
    /// </summary>
    public static Dictionary<string, List<string>> GetPermissionsByCategory()
    {
        var allPermissions = GetAllPermissions();
        return allPermissions
            .GroupBy(p => p.Split('.')[1]) // Group by second part (e.g., "Staff", "Schedules")
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
