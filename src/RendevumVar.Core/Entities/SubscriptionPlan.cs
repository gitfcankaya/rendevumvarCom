using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int TrialDays { get; set; } = 0;
    public bool HasFreeTrial { get; set; } = false;
    public int FreeTrialDays { get; set; } = 0;
    public int DurationMonths { get; set; }
    public int MaxStaff { get; set; } = -1;
    public int MaxAppointmentsPerMonth { get; set; } = -1;
    public int MaxLocations { get; set; } = 1;
    public int MaxServices { get; set; } = -1;
    public bool HasAdvancedAnalytics { get; set; } = false;
    public bool HasSMSNotifications { get; set; } = false;
    public bool HasEmailNotifications { get; set; } = true;
    public bool HasCustomBranding { get; set; } = false;
    public bool HasAPIAccess { get; set; } = false;
    public bool HasMultiLocation { get; set; } = false;
    public bool HasPackageManagement { get; set; } = false;
    public bool HasGoogleCalendarIntegration { get; set; } = false;
    public bool HasPaymentIntegration { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string Features { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string? Color { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}
