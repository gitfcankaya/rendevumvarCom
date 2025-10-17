using RendevumVar.Core.Enums;

namespace RendevumVar.Application.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DurationMonths { get; set; }
    public bool HasFreeTrial { get; set; }
    public int FreeTrialDays { get; set; }
    public bool IsActive { get; set; }
    public bool IsPopular { get; set; }
    public int SortOrder { get; set; }
    public List<string> Features { get; set; } = new();
    public string? Badge { get; set; }
    public string? Color { get; set; }
    public decimal Discount => OriginalPrice > 0 ? ((OriginalPrice - Price) / OriginalPrice) * 100 : 0;
}

public class CreateSubscriptionPlanDto
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
    public List<string> Features { get; set; } = new();
    public string? Badge { get; set; }
    public string? Color { get; set; }
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsFreeTrial { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public SubscriptionStatus Status { get; set; }
    public int DaysRemaining => (EndDate - DateTime.UtcNow).Days;
    public bool IsExpired => DateTime.UtcNow > EndDate;
}

public class CreateSubscriptionDto
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public bool IsFreeTrial { get; set; } = false;
    public string? PaymentReference { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubscriptionPaymentDto
{
    public Guid? UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.PayTR;
    public string? PaymentReference { get; set; }
}