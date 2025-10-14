using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class Payment : BaseEntity
{
    public Guid? UserId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; } = "PayTR";
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? PaymentDetails { get; set; } // JSON for additional payment info

    // Navigation properties
    public User? User { get; set; }
    public Appointment? Appointment { get; set; }
    public Subscription? Subscription { get; set; }
}
