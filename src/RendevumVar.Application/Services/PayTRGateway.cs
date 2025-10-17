using Microsoft.Extensions.Options;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;

namespace RendevumVar.Application.Services;

/// <summary>
/// PayTR payment gateway integration
/// https://www.paytr.com/
/// </summary>
public class PayTRGateway : IPaymentGateway
{
    private readonly PayTRConfiguration _config;
    private readonly HttpClient _httpClient;
    private const string PayTRApiUrl = "https://www.paytr.com/odeme/api/get-token";
    private const string PayTRPaymentUrl = "https://www.paytr.com/odeme/guvenli/";

    public string GatewayName => "PayTR";

    public PayTRGateway(IOptions<PayTRConfiguration> config, IHttpClientFactory httpClientFactory)
    {
        _config = config.Value;
        _httpClient = httpClientFactory.CreateClient("PayTR");
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto request, Guid userId)
    {
        if (!IsConfigured())
        {
            throw new InvalidOperationException("PayTR is not properly configured. Please check appsettings.json");
        }

        try
        {
            var merchantOid = $"{userId:N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var userIp = "127.0.0.1"; // Should be passed from controller
            var basketStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new[]
            {
                new[] { "Randevu Ödemesi", request.Amount.ToString("F2"), "1" }
            })));

            // Calculate PayTR hash
            var hashStr = $"{_config.MerchantId}{userIp}{merchantOid}{request.UserEmail ?? "customer@example.com"}" +
                         $"{request.Amount:F2}{basketStr}{_config.NoInstallment}{_config.MaxInstallment}" +
                         $"{request.Currency}{_config.TestMode}{_config.MerchantSalt}";

            var hash = GenerateHash(hashStr);

            var payload = new Dictionary<string, string>
            {
                { "merchant_id", _config.MerchantId },
                { "merchant_key", _config.MerchantKey },
                { "merchant_salt", _config.MerchantSalt },
                { "user_ip", userIp },
                { "merchant_oid", merchantOid },
                { "email", request.UserEmail ?? "customer@example.com" },
                { "payment_amount", ((int)(request.Amount * 100)).ToString() }, // Convert to kuruş
                { "payment_type", "card" },
                { "installment_count", _config.NoInstallment.ToString() },
                { "currency", request.Currency },
                { "test_mode", _config.TestMode.ToString() },
                { "no_installment", _config.NoInstallment.ToString() },
                { "max_installment", _config.MaxInstallment.ToString() },
                { "user_basket", basketStr },
                { "debug_on", "1" },
                { "merchant_ok_url", request.SuccessUrl ?? _config.SuccessUrl },
                { "merchant_fail_url", request.FailureUrl ?? _config.FailureUrl },
                { "timeout_limit", "30" },
                { "lang", "tr" },
                { "paytr_token", hash }
            };

            var content = new FormUrlEncodedContent(payload);
            var response = await _httpClient.PostAsync(PayTRApiUrl, content);
            var responseStr = await response.Content.ReadAsStringAsync();
            var paytrResponse = JsonSerializer.Deserialize<PayTRTokenResponse>(responseStr);

            if (paytrResponse?.Status == "success")
            {
                return new PaymentResponseDto
                {
                    PaymentId = Guid.NewGuid(),
                    Status = PaymentStatus.Pending,
                    TransactionId = merchantOid,
                    PaymentGateway = GatewayName,
                    PaymentReference = merchantOid,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    CreatedAt = DateTime.UtcNow,
                    PaymentUrl = $"{PayTRPaymentUrl}{paytrResponse.Token}",
                    Token = paytrResponse.Token
                };
            }
            else
            {
                return new PaymentResponseDto
                {
                    PaymentId = Guid.NewGuid(),
                    Status = PaymentStatus.Failed,
                    FailureReason = paytrResponse?.Reason ?? "Failed to create PayTR payment",
                    Amount = request.Amount,
                    Currency = request.Currency,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            return new PaymentResponseDto
            {
                PaymentId = Guid.NewGuid(),
                Status = PaymentStatus.Failed,
                FailureReason = $"PayTR error: {ex.Message}",
                Amount = request.Amount,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public Task<PaymentResponseDto> VerifyCallbackAsync(PaymentCallbackDto callback)
    {
        // Verify PayTR callback hash
        var hashStr = $"{callback.MerchantOid}{_config.MerchantSalt}{callback.Status}{callback.TotalAmount}";
        var hash = GenerateHash(hashStr);

        if (hash != callback.Hash)
        {
            return Task.FromResult(new PaymentResponseDto
            {
                PaymentId = Guid.NewGuid(),
                Status = PaymentStatus.Failed,
                FailureReason = "Invalid callback hash",
                CreatedAt = DateTime.UtcNow
            });
        }

        var status = callback.Status == "success" ? PaymentStatus.Completed : PaymentStatus.Failed;

        return Task.FromResult(new PaymentResponseDto
        {
            PaymentId = Guid.NewGuid(),
            Status = status,
            TransactionId = callback.MerchantOid,
            PaymentGateway = GatewayName,
            PaymentReference = callback.MerchantOid,
            FailureReason = callback.FailedReasonMsg,
            Amount = decimal.Parse(callback.TotalAmount ?? "0") / 100, // Convert from kuruş
            Currency = callback.Currency ?? "TRY",
            CreatedAt = DateTime.UtcNow,
            PaymentDate = status == PaymentStatus.Completed ? DateTime.UtcNow : null
        });
    }

    public async Task<PaymentResponseDto> CheckPaymentStatusAsync(string transactionId)
    {
        // PayTR doesn't have a direct status check API
        // Status is received via callback
        await Task.CompletedTask;

        return new PaymentResponseDto
        {
            PaymentId = Guid.NewGuid(),
            Status = PaymentStatus.Pending,
            TransactionId = transactionId,
            PaymentGateway = GatewayName,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> RefundPaymentAsync(string transactionId, decimal amount, string? reason = null)
    {
        // PayTR refund implementation would go here
        // Requires separate API call to PayTR refund endpoint
        await Task.CompletedTask;
        return false; // Not implemented in this version
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_config.MerchantId) &&
               !string.IsNullOrEmpty(_config.MerchantKey) &&
               !string.IsNullOrEmpty(_config.MerchantSalt) &&
               _config.MerchantId != "PAYTR_MERCHANT_ID_HERE";
    }

    private string GenerateHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private class PayTRTokenResponse
    {
        public string? Status { get; set; }
        public string? Token { get; set; }
        public string? Reason { get; set; }
    }
}

public class PayTRConfiguration
{
    public string MerchantId { get; set; } = "PAYTR_MERCHANT_ID_HERE";
    public string MerchantKey { get; set; } = "PAYTR_MERCHANT_KEY_HERE";
    public string MerchantSalt { get; set; } = "PAYTR_MERCHANT_SALT_HERE";
    public string SuccessUrl { get; set; } = "https://yourdomain.com/payment/success";
    public string FailureUrl { get; set; } = "https://yourdomain.com/payment/failure";
    public int TestMode { get; set; } = 1; // 1 for test, 0 for production
    public int NoInstallment { get; set; } = 1; // No installment
    public int MaxInstallment { get; set; } = 0; // Max installment
}
