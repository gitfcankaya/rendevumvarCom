using RendevumVar.Core.Enums;

namespace RendevumVar.Application.DTOs;

public class CreatePaymentDto
{
    public Guid? AppointmentId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;

    // User info (for PayTR)
    public string? UserEmail { get; set; }

    // Card details (for fake POS)
    public string? CardHolderName { get; set; }
    public string? CardNumber { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }

    // Callback URL for payment gateway
    public string? SuccessUrl { get; set; }
    public string? FailureUrl { get; set; }
}

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime CreatedAt { get; set; }
    public DateTime? PaymentDate { get; set; }

    // For redirect-based payments (like PayTR)
    public string? PaymentUrl { get; set; }
    public string? Token { get; set; }
}

public class PaymentDetailDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? RefundDate { get; set; }
    public decimal? RefundAmount { get; set; }

    // Related entities
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
}

public class RefundPaymentDto
{
    public decimal? RefundAmount { get; set; } // If null, full refund
    public string? Reason { get; set; }
}

public class PaymentCallbackDto
{
    // PayTR format
    public string? MerchantOid { get; set; } // Our payment reference
    public string? Status { get; set; } // success, failed
    public string? TotalAmount { get; set; }
    public string? Hash { get; set; } // Security hash
    public string? FailedReasonCode { get; set; }
    public string? FailedReasonMsg { get; set; }
    public string? TestMode { get; set; }
    public string? PaymentType { get; set; }
    public string? Currency { get; set; }
    public string? MerchantId { get; set; }
    public string? PaymentAmount { get; set; }
}

public class PaymentStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int RefundedPayments { get; set; }
    public decimal AveragePaymentAmount { get; set; }
    public Dictionary<string, int> PaymentsByMethod { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}
