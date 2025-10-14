using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DurationMonths { get; set; }
    public bool HasFreeTrial { get; set; } = false;
    public int FreeTrialDays { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsPopular { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string Features { get; set; } = string.Empty; // JSON string
    public string? Badge { get; set; }
    public string? Color { get; set; }
    
    // Navigation properties
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}