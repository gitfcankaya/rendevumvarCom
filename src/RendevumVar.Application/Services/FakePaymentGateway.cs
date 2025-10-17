using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Enums;

namespace RendevumVar.Application.Services;

/// <summary>
/// Fake payment gateway for development and testing
/// Simulates real payment behavior with test card numbers
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
    public string GatewayName => "FakePOS";

    // Test card numbers
    private static readonly Dictionary<string, string> TestCards = new()
    {
        { "4242424242424242", "success" },           // Successful payment
        { "4000000000000002", "declined" },          // Card declined
        { "4000000000009995", "insufficient_funds" }, // Insufficient funds
        { "4000000000000069", "expired_card" },      // Expired card
        { "4000000000000127", "incorrect_cvc" },     // Incorrect CVC
        { "4000000000000119", "processing_error" }   // Processing error
    };

    public Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto request, Guid userId)
    {
        // Validate card number
        var cardNumber = request.CardNumber?.Replace(" ", "");
        if (string.IsNullOrEmpty(cardNumber))
        {
            return Task.FromResult(CreateFailedResponse(request, "Card number is required"));
        }

        // Check if it's a test card
        if (!TestCards.TryGetValue(cardNumber, out var testResult))
        {
            // Unknown card - treat as invalid
            return Task.FromResult(CreateFailedResponse(request, "Invalid card number"));
        }

        // Simulate processing delay
        Thread.Sleep(1000);

        // Generate transaction ID
        var transactionId = $"FAKE-{Guid.NewGuid():N}";
        var paymentReference = $"REF-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        // Handle different test card scenarios
        var response = testResult switch
        {
            "success" => new PaymentResponseDto
            {
                PaymentId = Guid.NewGuid(), // Will be replaced by actual payment ID
                Status = PaymentStatus.Completed,
                TransactionId = transactionId,
                PaymentGateway = GatewayName,
                PaymentReference = paymentReference,
                Amount = request.Amount,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow
            },
            "declined" => CreateFailedResponse(request, "Card was declined"),
            "insufficient_funds" => CreateFailedResponse(request, "Insufficient funds"),
            "expired_card" => CreateFailedResponse(request, "Card has expired"),
            "incorrect_cvc" => CreateFailedResponse(request, "Incorrect CVC code"),
            "processing_error" => CreateFailedResponse(request, "Payment processing error"),
            _ => CreateFailedResponse(request, "Unknown error")
        };

        return Task.FromResult(response);
    }

    public Task<PaymentResponseDto> VerifyCallbackAsync(PaymentCallbackDto callback)
    {
        // Fake gateway doesn't use callbacks, always return success
        var response = new PaymentResponseDto
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Completed,
            TransactionId = callback.MerchantOid ?? "FAKE-CALLBACK",
            PaymentGateway = GatewayName,
            CreatedAt = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    public Task<PaymentResponseDto> CheckPaymentStatusAsync(string transactionId)
    {
        // Simulate checking payment status
        // In fake mode, we consider all transactions as completed
        var response = new PaymentResponseDto
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Completed,
            TransactionId = transactionId,
            PaymentGateway = GatewayName,
            CreatedAt = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow
        };

        return Task.FromResult(response);
    }

    public Task<bool> RefundPaymentAsync(string transactionId, decimal amount, string? reason = null)
    {
        // Fake gateway always allows refunds
        Thread.Sleep(500); // Simulate processing
        return Task.FromResult(true);
    }

    public bool IsConfigured()
    {
        // Fake gateway is always configured
        return true;
    }

    private PaymentResponseDto CreateFailedResponse(CreatePaymentDto request, string failureReason)
    {
        return new PaymentResponseDto
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Failed,
            TransactionId = $"FAKE-FAILED-{Guid.NewGuid():N}",
            PaymentGateway = GatewayName,
            FailureReason = failureReason,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Get list of test card numbers and their expected behavior
    /// </summary>
    public static Dictionary<string, string> GetTestCards()
    {
        return new Dictionary<string, string>(TestCards);
    }
}
