using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Interfaces;

/// <summary>
/// Interface for payment gateway implementations (PayTR, Iyzico, Stripe, etc.)
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Gateway name (e.g., "PayTR", "Iyzico", "FakePOS")
    /// </summary>
    string GatewayName { get; }

    /// <summary>
    /// Create a payment and return payment URL or transaction details
    /// </summary>
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto request, Guid userId);

    /// <summary>
    /// Verify payment callback from gateway (webhook)
    /// </summary>
    Task<PaymentResponseDto> VerifyCallbackAsync(PaymentCallbackDto callback);

    /// <summary>
    /// Check payment status from gateway
    /// </summary>
    Task<PaymentResponseDto> CheckPaymentStatusAsync(string transactionId);

    /// <summary>
    /// Refund a payment
    /// </summary>
    Task<bool> RefundPaymentAsync(string transactionId, decimal amount, string? reason = null);

    /// <summary>
    /// Validate payment configuration
    /// </summary>
    bool IsConfigured();
}
