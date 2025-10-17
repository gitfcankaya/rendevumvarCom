using Microsoft.Extensions.Options;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Repositories;

namespace RendevumVar.Application.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto request, Guid userId);
    Task<PaymentResponseDto> ProcessCallbackAsync(PaymentCallbackDto callback);
    Task<PaymentDetailDto?> GetPaymentByIdAsync(Guid id);
    Task<PaymentDetailDto?> GetPaymentByTransactionIdAsync(string transactionId);
    Task<List<PaymentDetailDto>> GetUserPaymentsAsync(Guid userId);
    Task<List<PaymentDetailDto>> GetSalonPaymentsAsync(Guid salonId);
    Task<PaymentResponseDto> RefundPaymentAsync(Guid paymentId, RefundPaymentDto request, Guid userId);
    Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(Guid? salonId = null);
    Task<List<string>> GetTestCardNumbersAsync();
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly PaymentConfiguration _config;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IAppointmentRepository appointmentRepository,
        IPaymentGateway paymentGateway,
        IOptions<PaymentConfiguration> config)
    {
        _paymentRepository = paymentRepository;
        _appointmentRepository = appointmentRepository;
        _paymentGateway = paymentGateway;
        _config = config.Value;
    }

    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto request, Guid userId)
    {
        // Validate request
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero");
        }

        // Validate appointment if provided
        if (request.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value);
            if (appointment == null)
            {
                throw new ArgumentException("Appointment not found");
            }

            if (appointment.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You can only pay for your own appointments");
            }

            // Check if already paid
            var existingPayment = await _paymentRepository.GetByAppointmentIdAsync(request.AppointmentId.Value);
            if (existingPayment != null && existingPayment.Status == PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Appointment is already paid");
            }
        }

        // Create payment via gateway
        var gatewayResponse = await _paymentGateway.CreatePaymentAsync(request, userId);

        // Save payment to database
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AppointmentId = request.AppointmentId,
            SubscriptionId = request.SubscriptionId,
            Amount = request.Amount,
            Currency = request.Currency,
            Method = request.Method,
            Status = gatewayResponse.Status,
            TransactionId = gatewayResponse.TransactionId,
            PaymentGateway = _paymentGateway.GatewayName,
            PaymentReference = gatewayResponse.PaymentReference,
            FailureReason = gatewayResponse.FailureReason,
            PaymentDate = gatewayResponse.PaymentDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var savedPayment = await _paymentRepository.CreateAsync(payment);
        gatewayResponse.PaymentId = savedPayment.Id;

        // Update appointment status if payment successful
        if (gatewayResponse.Status == PaymentStatus.Completed && request.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                await _appointmentRepository.UpdateAsync(appointment);
            }
        }

        return gatewayResponse;
    }

    public async Task<PaymentResponseDto> ProcessCallbackAsync(PaymentCallbackDto callback)
    {
        // Verify callback with gateway
        var gatewayResponse = await _paymentGateway.VerifyCallbackAsync(callback);

        // Find payment by transaction ID
        var payment = await _paymentRepository.GetByTransactionIdAsync(
            callback.MerchantOid ?? gatewayResponse.TransactionId ?? "");

        if (payment == null)
        {
            throw new ArgumentException("Payment not found");
        }

        // Update payment status
        payment.Status = gatewayResponse.Status;
        payment.PaymentDate = gatewayResponse.PaymentDate;
        payment.FailureReason = gatewayResponse.FailureReason;
        await _paymentRepository.UpdateAsync(payment);

        // Update appointment status if payment successful
        if (gatewayResponse.Status == PaymentStatus.Completed && payment.AppointmentId.HasValue)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId.Value);
            if (appointment != null && appointment.Status == AppointmentStatus.Pending)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                await _appointmentRepository.UpdateAsync(appointment);
            }
        }

        gatewayResponse.PaymentId = payment.Id;
        return gatewayResponse;
    }

    public async Task<PaymentDetailDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null) return null;

        return MapToDetailDto(payment);
    }

    public async Task<PaymentDetailDto?> GetPaymentByTransactionIdAsync(string transactionId)
    {
        var payment = await _paymentRepository.GetByTransactionIdAsync(transactionId);
        if (payment == null) return null;

        return MapToDetailDto(payment);
    }

    public async Task<List<PaymentDetailDto>> GetUserPaymentsAsync(Guid userId)
    {
        var payments = await _paymentRepository.GetByUserIdAsync(userId);
        return payments.Select(MapToDetailDto).ToList();
    }

    public async Task<List<PaymentDetailDto>> GetSalonPaymentsAsync(Guid salonId)
    {
        var payments = await _paymentRepository.GetBySalonIdAsync(salonId);
        return payments.Select(MapToDetailDto).ToList();
    }

    public async Task<PaymentResponseDto> RefundPaymentAsync(Guid paymentId, RefundPaymentDto request, Guid userId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            throw new ArgumentException("Payment not found");
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Only completed payments can be refunded");
        }

        if (payment.RefundDate.HasValue)
        {
            throw new InvalidOperationException("Payment is already refunded");
        }

        // Determine refund amount
        var refundAmount = request.RefundAmount ?? payment.Amount;
        if (refundAmount > payment.Amount)
        {
            throw new ArgumentException("Refund amount cannot exceed payment amount");
        }

        // Process refund via gateway
        var success = await _paymentGateway.RefundPaymentAsync(
            payment.TransactionId ?? "",
            refundAmount,
            request.Reason);

        if (success)
        {
            payment.Status = PaymentStatus.Refunded;
            payment.RefundAmount = refundAmount;
            payment.RefundDate = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            // Update appointment status
            if (payment.AppointmentId.HasValue)
            {
                var appointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.Status = AppointmentStatus.Cancelled;
                    await _appointmentRepository.UpdateAsync(appointment);
                }
            }

            return new PaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = PaymentStatus.Refunded,
                TransactionId = payment.TransactionId,
                PaymentGateway = payment.PaymentGateway,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CreatedAt = payment.CreatedAt
            };
        }

        throw new InvalidOperationException("Refund failed");
    }

    public async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(Guid? salonId = null)
    {
        var statusStats = await _paymentRepository.GetPaymentStatsByStatusAsync(salonId);
        var totalRevenue = await _paymentRepository.GetTotalRevenueAsync(salonId);

        var totalPayments = statusStats.Values.Sum();
        var successfulPayments = statusStats.GetValueOrDefault(PaymentStatus.Completed, 0);
        var failedPayments = statusStats.GetValueOrDefault(PaymentStatus.Failed, 0);
        var refundedPayments = statusStats.GetValueOrDefault(PaymentStatus.Refunded, 0);

        return new PaymentStatisticsDto
        {
            TotalRevenue = totalRevenue,
            TotalPayments = totalPayments,
            SuccessfulPayments = successfulPayments,
            FailedPayments = failedPayments,
            RefundedPayments = refundedPayments,
            AveragePaymentAmount = successfulPayments > 0 ? totalRevenue / successfulPayments : 0
        };
    }

    public Task<List<string>> GetTestCardNumbersAsync()
    {
        var testCards = FakePaymentGateway.GetTestCards();
        return Task.FromResult(testCards.Keys.ToList());
    }

    private PaymentDetailDto MapToDetailDto(Payment payment)
    {
        return new PaymentDetailDto
        {
            Id = payment.Id,
            UserId = payment.UserId,
            AppointmentId = payment.AppointmentId,
            SubscriptionId = payment.SubscriptionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaymentGateway = payment.PaymentGateway,
            PaymentReference = payment.PaymentReference,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            PaymentDate = payment.PaymentDate,
            RefundDate = payment.RefundDate,
            RefundAmount = payment.RefundAmount,
            UserEmail = payment.User?.Email,
            UserName = payment.User != null ? $"{payment.User.FirstName} {payment.User.LastName}" : null
        };
    }
}

public class PaymentConfiguration
{
    public string DefaultGateway { get; set; } = "FakePOS"; // FakePOS, PayTR, Iyzico
    public bool EnableDeposit { get; set; } = true;
    public decimal DepositPercentage { get; set; } = 20; // 20% deposit
    public bool EnableNoShowFee { get; set; } = true;
    public decimal NoShowFeePercentage { get; set; } = 50; // 50% fee for no-show
    public int RefundWindowHours { get; set; } = 24; // Can refund up to 24 hours before appointment
}
