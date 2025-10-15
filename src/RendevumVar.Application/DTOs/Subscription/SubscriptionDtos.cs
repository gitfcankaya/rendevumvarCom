using RendevumVar.Core.Enums;

namespace RendevumVar.Application.DTOs.Subscription;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int TrialDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsPopular { get; set; }
    public int SortOrder { get; set; }
    public string Features { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string? Color { get; set; }

    // Feature Limits
    public int MaxStaff { get; set; }
    public int MaxAppointmentsPerMonth { get; set; }
    public int MaxLocations { get; set; }
    public int MaxServices { get; set; }

    // Feature Flags
    public bool HasAdvancedAnalytics { get; set; }
    public bool HasSMSNotifications { get; set; }
    public bool HasEmailNotifications { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasMultiLocation { get; set; }
    public bool HasPackageManagement { get; set; }
    public bool HasGoogleCalendarIntegration { get; set; }
    public bool HasPaymentIntegration { get; set; }
}

public class CurrentSubscriptionDto
{
    public Guid Id { get; set; }
    public SubscriptionPlanDto Plan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public bool AutoRenew { get; set; }
    public int DaysUntilExpiry { get; set; }
    public bool IsTrialing => Status == SubscriptionStatus.Trialing;
}

public class CreateTrialSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
}

public class UpgradeSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid NewPlanId { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class DowngradeSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public Guid NewPlanId { get; set; }
}

public class CancelSubscriptionRequest
{
    public Guid TenantId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class FeatureLimitsDto
{
    public int MaxStaff { get; set; }
    public int MaxAppointmentsPerMonth { get; set; }
    public int MaxLocations { get; set; }
    public int MaxServices { get; set; }
    public int CurrentStaffCount { get; set; }
    public int CurrentAppointmentsThisMonth { get; set; }
    public int CurrentLocationsCount { get; set; }
    public int CurrentServicesCount { get; set; }
    public bool CanAddStaff => MaxStaff == -1 || CurrentStaffCount < MaxStaff;
    public bool CanAddAppointment => MaxAppointmentsPerMonth == -1 || CurrentAppointmentsThisMonth < MaxAppointmentsPerMonth;
    public bool CanAddLocation => MaxLocations == -1 || CurrentLocationsCount < MaxLocations;
    public bool CanAddService => MaxServices == -1 || CurrentServicesCount < MaxServices;
}

public class ProrationCalculationDto
{
    public decimal CurrentPlanCost { get; set; }
    public decimal NewPlanCost { get; set; }
    public decimal ProrationCredit { get; set; }
    public decimal AmountToPay { get; set; }
    public int DaysRemainingInCycle { get; set; }
    public int TotalDaysInCycle { get; set; }
}
