using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFreeTrial { get; set; } = false;
    public decimal AmountPaid { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}