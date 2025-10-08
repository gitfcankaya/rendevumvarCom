namespace RendevumVar.Core.Entities;

public class Payment : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, Cash, BankTransfer
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public decimal? RefundAmount { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
}
