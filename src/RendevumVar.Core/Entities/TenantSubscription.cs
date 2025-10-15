using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

/// <summary>
/// Tenant-specific subscription record (Phase 2)
/// Replaces user-level subscriptions with tenant-level subscriptions
/// </summary>
public class TenantSubscription : BaseEntity
{
    // Tenant reference
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Plan reference
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    // Status
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    
    // Dates
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    
    // Payment
    public string? PaymentMethodId { get; set; } // External payment method ID (e.g., from PayTR)
    public bool AutoRenew { get; set; } = true;
    
    // Cancellation
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
