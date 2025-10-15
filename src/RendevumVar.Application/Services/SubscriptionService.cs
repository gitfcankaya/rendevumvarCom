using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs.Subscription;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionPlanRepository _planRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ISubscriptionPlanRepository planRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        ApplicationDbContext context,
        ILogger<SubscriptionService> logger)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync()
    {
        var plans = await _planRepository.GetActiveActivePlansAsync();
        return plans.Select(MapToDto);
    }

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId)
    {
        var plan = await _planRepository.GetByIdAsync(planId);
        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<CurrentSubscriptionDto?> GetCurrentSubscriptionAsync(Guid tenantId)
    {
        var subscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(tenantId);
        
        if (subscription == null)
            return null;

        var daysUntilExpiry = subscription.EndDate.HasValue
            ? (subscription.EndDate.Value - DateTime.UtcNow).Days
            : 0;

        return new CurrentSubscriptionDto
        {
            Id = subscription.Id,
            Plan = MapToDto(subscription.SubscriptionPlan),
            Status = subscription.Status,
            BillingCycle = subscription.BillingCycle,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            TrialEndDate = subscription.TrialEndDate,
            NextBillingDate = subscription.NextBillingDate,
            AutoRenew = subscription.AutoRenew,
            DaysUntilExpiry = daysUntilExpiry
        };
    }

    public async Task<CurrentSubscriptionDto> CreateTrialSubscriptionAsync(CreateTrialSubscriptionRequest request)
    {
        _logger.LogInformation("Creating trial subscription for tenant {TenantId}", request.TenantId);

        // Check if tenant already has a subscription
        var existingSubscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(request.TenantId);
        if (existingSubscription != null)
        {
            throw new InvalidOperationException("Tenant already has an active subscription");
        }

        var plan = await _planRepository.GetByIdAsync(request.SubscriptionPlanId);
        if (plan == null)
        {
            throw new ArgumentException("Subscription plan not found");
        }

        var now = DateTime.UtcNow;
        var trialEndDate = now.AddDays(plan.TrialDays);

        var subscription = new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            SubscriptionPlanId = request.SubscriptionPlanId,
            Status = SubscriptionStatus.Trialing,
            BillingCycle = BillingCycle.Monthly,
            StartDate = now,
            TrialEndDate = trialEndDate,
            AutoRenew = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "System",
            IsDeleted = false
        };

        await _subscriptionRepository.AddAsync(subscription);

        _logger.LogInformation("Trial subscription created successfully for tenant {TenantId}", request.TenantId);

        return new CurrentSubscriptionDto
        {
            Id = subscription.Id,
            Plan = MapToDto(plan),
            Status = subscription.Status,
            BillingCycle = subscription.BillingCycle,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            TrialEndDate = subscription.TrialEndDate,
            NextBillingDate = subscription.NextBillingDate,
            AutoRenew = subscription.AutoRenew,
            DaysUntilExpiry = plan.TrialDays
        };
    }

    public async Task<CurrentSubscriptionDto> UpgradeSubscriptionAsync(UpgradeSubscriptionRequest request)
    {
        _logger.LogInformation("Upgrading subscription for tenant {TenantId} to plan {PlanId}",
            request.TenantId, request.NewPlanId);

        var currentSubscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(request.TenantId);
        if (currentSubscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        var newPlan = await _planRepository.GetByIdAsync(request.NewPlanId);
        if (newPlan == null)
        {
            throw new ArgumentException("New subscription plan not found");
        }

        // Calculate proration
        var proration = await CalculateProrationAsync(request.TenantId, request.NewPlanId);

        var now = DateTime.UtcNow;
        
        // Update current subscription
        currentSubscription.SubscriptionPlanId = request.NewPlanId;
        currentSubscription.BillingCycle = request.BillingCycle;
        currentSubscription.Status = SubscriptionStatus.Active;
        currentSubscription.PaymentMethodId = request.PaymentMethodId;
        currentSubscription.UpdatedAt = now;
        currentSubscription.UpdatedBy = "System";

        // If currently trialing, set end date and next billing date
        if (currentSubscription.Status == SubscriptionStatus.Trialing)
        {
            var billingMonths = request.BillingCycle == BillingCycle.Annual ? 12 : 1;
            currentSubscription.EndDate = now.AddMonths(billingMonths);
            currentSubscription.NextBillingDate = currentSubscription.EndDate;
        }

        await _subscriptionRepository.UpdateAsync(currentSubscription);

        _logger.LogInformation("Subscription upgraded successfully for tenant {TenantId}", request.TenantId);

        return await GetCurrentSubscriptionAsync(request.TenantId) 
            ?? throw new InvalidOperationException("Failed to retrieve updated subscription");
    }

    public async Task<CurrentSubscriptionDto> DowngradeSubscriptionAsync(DowngradeSubscriptionRequest request)
    {
        _logger.LogInformation("Downgrading subscription for tenant {TenantId} to plan {PlanId}",
            request.TenantId, request.NewPlanId);

        var currentSubscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(request.TenantId);
        if (currentSubscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        var newPlan = await _planRepository.GetByIdAsync(request.NewPlanId);
        if (newPlan == null)
        {
            throw new ArgumentException("New subscription plan not found");
        }

        // Downgrade takes effect at the end of current billing cycle
        currentSubscription.SubscriptionPlanId = request.NewPlanId;
        currentSubscription.UpdatedAt = DateTime.UtcNow;
        currentSubscription.UpdatedBy = "System";

        await _subscriptionRepository.UpdateAsync(currentSubscription);

        _logger.LogInformation("Subscription will be downgraded at end of billing cycle for tenant {TenantId}",
            request.TenantId);

        return await GetCurrentSubscriptionAsync(request.TenantId)
            ?? throw new InvalidOperationException("Failed to retrieve updated subscription");
    }

    public async Task CancelSubscriptionAsync(CancelSubscriptionRequest request)
    {
        _logger.LogInformation("Cancelling subscription for tenant {TenantId}", request.TenantId);

        var subscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(request.TenantId);
        if (subscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        var now = DateTime.UtcNow;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.AutoRenew = false;
        subscription.CancelledAt = now;
        subscription.CancellationReason = request.Reason;
        subscription.UpdatedAt = now;
        subscription.UpdatedBy = "System";

        await _subscriptionRepository.UpdateAsync(subscription);

        _logger.LogInformation("Subscription cancelled for tenant {TenantId}", request.TenantId);
    }

    public async Task<FeatureLimitsDto> GetFeatureLimitsAsync(Guid tenantId)
    {
        var subscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(tenantId);
        if (subscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        var plan = subscription.SubscriptionPlan;

        // Get current usage counts
        var currentStaffCount = await _context.Staff.CountAsync(s => s.TenantId == tenantId && !s.IsDeleted);
        var currentLocationsCount = await _context.Salons.CountAsync(s => s.TenantId == tenantId && !s.IsDeleted);
        var currentServicesCount = await _context.Services.CountAsync(s => s.TenantId == tenantId && !s.IsDeleted);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var currentAppointmentsThisMonth = await _context.Appointments.CountAsync(a =>
            a.TenantId == tenantId &&
            a.StartTime >= startOfMonth &&
            !a.IsDeleted);

        return new FeatureLimitsDto
        {
            MaxStaff = plan.MaxStaff,
            MaxAppointmentsPerMonth = plan.MaxAppointmentsPerMonth,
            MaxLocations = plan.MaxLocations,
            MaxServices = plan.MaxServices,
            CurrentStaffCount = currentStaffCount,
            CurrentAppointmentsThisMonth = currentAppointmentsThisMonth,
            CurrentLocationsCount = currentLocationsCount,
            CurrentServicesCount = currentServicesCount
        };
    }

    public async Task<bool> CheckFeatureLimitAsync(Guid tenantId, string featureName)
    {
        var limits = await GetFeatureLimitsAsync(tenantId);

        return featureName.ToLower() switch
        {
            "staff" => limits.CanAddStaff,
            "appointment" => limits.CanAddAppointment,
            "location" => limits.CanAddLocation,
            "service" => limits.CanAddService,
            _ => true
        };
    }

    public async Task<ProrationCalculationDto> CalculateProrationAsync(Guid tenantId, Guid newPlanId)
    {
        var currentSubscription = await _subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(tenantId);
        if (currentSubscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        var currentPlan = currentSubscription.SubscriptionPlan;
        var newPlan = await _planRepository.GetByIdAsync(newPlanId);
        if (newPlan == null)
        {
            throw new ArgumentException("New subscription plan not found");
        }

        var currentPlanCost = currentSubscription.BillingCycle == BillingCycle.Annual
            ? currentPlan.AnnualPrice
            : currentPlan.MonthlyPrice;

        var newPlanCost = currentSubscription.BillingCycle == BillingCycle.Annual
            ? newPlan.AnnualPrice
            : newPlan.MonthlyPrice;

        // Calculate days remaining in current billing cycle
        var daysRemainingInCycle = currentSubscription.NextBillingDate.HasValue
            ? (currentSubscription.NextBillingDate.Value - DateTime.UtcNow).Days
            : 0;

        var totalDaysInCycle = currentSubscription.BillingCycle == BillingCycle.Annual ? 365 : 30;

        // Calculate proration credit
        var prorationCredit = (currentPlanCost / totalDaysInCycle) * daysRemainingInCycle;
        var amountToPay = newPlanCost - prorationCredit;

        return new ProrationCalculationDto
        {
            CurrentPlanCost = currentPlanCost,
            NewPlanCost = newPlanCost,
            ProrationCredit = prorationCredit,
            AmountToPay = Math.Max(0, amountToPay),
            DaysRemainingInCycle = daysRemainingInCycle,
            TotalDaysInCycle = totalDaysInCycle
        };
    }

    private static SubscriptionPlanDto MapToDto(SubscriptionPlan plan)
    {
        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice,
            AnnualPrice = plan.AnnualPrice,
            TrialDays = plan.TrialDays,
            IsActive = plan.IsActive,
            IsPopular = plan.IsPopular,
            SortOrder = plan.SortOrder,
            Features = plan.Features,
            Badge = plan.Badge,
            Color = plan.Color,
            MaxStaff = plan.MaxStaff,
            MaxAppointmentsPerMonth = plan.MaxAppointmentsPerMonth,
            MaxLocations = plan.MaxLocations,
            MaxServices = plan.MaxServices,
            HasAdvancedAnalytics = plan.HasAdvancedAnalytics,
            HasSMSNotifications = plan.HasSMSNotifications,
            HasEmailNotifications = plan.HasEmailNotifications,
            HasCustomBranding = plan.HasCustomBranding,
            HasAPIAccess = plan.HasAPIAccess,
            HasMultiLocation = plan.HasMultiLocation,
            HasPackageManagement = plan.HasPackageManagement,
            HasGoogleCalendarIntegration = plan.HasGoogleCalendarIntegration,
            HasPaymentIntegration = plan.HasPaymentIntegration
        };
    }
}
