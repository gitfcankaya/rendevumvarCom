using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Services;
using RendevumVar.Core.Enums;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PaymentResponseDto>> CreatePayment([FromBody] CreatePaymentDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Set user email for PayTR
            if (string.IsNullOrEmpty(request.UserEmail))
            {
                request.UserEmail = userEmail;
            }

            var response = await _paymentService.CreatePaymentAsync(request, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, new { error = "An error occurred while processing the payment" });
        }
    }

    /// <summary>
    /// Payment callback webhook (for PayTR, Iyzico, etc.)
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentCallback([FromForm] PaymentCallbackDto callback)
    {
        try
        {
            _logger.LogInformation("Payment callback received: {MerchantOid}, Status: {Status}",
                callback.MerchantOid, callback.Status);

            var response = await _paymentService.ProcessCallbackAsync(callback);

            // PayTR expects "OK" response
            return Ok("OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment callback");
            return Ok("OK"); // Still return OK to prevent retries
        }
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PaymentDetailDto>> GetPaymentById(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { error = "Payment not found" });
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Check authorization
            if (payment.UserId != userId && userRole != UserRole.Admin.ToString())
            {
                return Forbid();
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment {PaymentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the payment" });
        }
    }

    /// <summary>
    /// Get user's payment history
    /// </summary>
    [HttpGet("my-payments")]
    [Authorize]
    public async Task<ActionResult<List<PaymentDetailDto>>> GetMyPayments()
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var payments = await _paymentService.GetUserPaymentsAsync(userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user payments");
            return StatusCode(500, new { error = "An error occurred while retrieving payments" });
        }
    }

    /// <summary>
    /// Get salon's payment history (salon owner only)
    /// </summary>
    [HttpGet("salon/{salonId}")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<List<PaymentDetailDto>>> GetSalonPayments(Guid salonId)
    {
        try
        {
            // TODO: Verify salon ownership
            var payments = await _paymentService.GetSalonPaymentsAsync(salonId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salon payments for salon {SalonId}", salonId);
            return StatusCode(500, new { error = "An error occurred while retrieving payments" });
        }
    }

    /// <summary>
    /// Refund a payment
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize]
    public async Task<ActionResult<PaymentResponseDto>> RefundPayment(Guid id, [FromBody] RefundPaymentDto request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _paymentService.RefundPaymentAsync(id, request, userId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", id);
            return StatusCode(500, new { error = "An error occurred while processing the refund" });
        }
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "SalonOwner,Admin")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetPaymentStatistics([FromQuery] Guid? salonId = null)
    {
        try
        {
            var stats = await _paymentService.GetPaymentStatisticsAsync(salonId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics");
            return StatusCode(500, new { error = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get test card numbers (development/testing only)
    /// </summary>
    [HttpGet("test-cards")]
    [AllowAnonymous]
    public async Task<ActionResult<List<string>>> GetTestCards()
    {
        var testCards = await _paymentService.GetTestCardNumbersAsync();
        return Ok(new
        {
            cards = new Dictionary<string, string>
            {
                { "4242424242424242", "Successful payment" },
                { "4000000000000002", "Card declined" },
                { "4000000000009995", "Insufficient funds" },
                { "4000000000000069", "Expired card" },
                { "4000000000000127", "Incorrect CVC" },
                { "4000000000000119", "Processing error" }
            },
            note = "These test card numbers only work with FakePOS gateway in development mode"
        });
    }
}
