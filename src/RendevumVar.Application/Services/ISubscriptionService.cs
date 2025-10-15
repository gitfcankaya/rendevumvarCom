using RendevumVar.Application.DTOs.Subscription;

namespace RendevumVar.Application.Services;

public interface ISubscriptionService
{
    // Plan Management
    Task<IEnumerable<SubscriptionPlanDto>> GetAvailablePlansAsync();
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId);

    // Subscription Management
    Task<CurrentSubscriptionDto?> GetCurrentSubscriptionAsync(Guid tenantId);
    Task<CurrentSubscriptionDto> CreateTrialSubscriptionAsync(CreateTrialSubscriptionRequest request);
    Task<CurrentSubscriptionDto> UpgradeSubscriptionAsync(UpgradeSubscriptionRequest request);
    Task<CurrentSubscriptionDto> DowngradeSubscriptionAsync(DowngradeSubscriptionRequest request);
    Task CancelSubscriptionAsync(CancelSubscriptionRequest request);

    // Feature Limits
    Task<FeatureLimitsDto> GetFeatureLimitsAsync(Guid tenantId);
    Task<bool> CheckFeatureLimitAsync(Guid tenantId, string featureName);

    // Billing & Proration
    Task<ProrationCalculationDto> CalculateProrationAsync(Guid tenantId, Guid newPlanId);
}
