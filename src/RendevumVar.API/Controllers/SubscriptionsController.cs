using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs.Subscription;
using RendevumVar.Application.Services;
using RendevumVar.Core.Repositories;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IInvoiceRepository invoiceRepository,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _invoiceRepository = invoiceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans()
    {
        try
        {
            var plans = await _subscriptionService.GetAvailablePlansAsync();
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            return StatusCode(500, new { message = "An error occurred while retrieving subscription plans" });
        }
    }

    /// <summary>
    /// Get specific subscription plan by ID
    /// </summary>
    [HttpGet("plans/{planId}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlan(Guid planId)
    {
        try
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
                return NotFound(new { message = "Subscription plan not found" });

            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plan {PlanId}", planId);
            return StatusCode(500, new { message = "An error occurred while retrieving the subscription plan" });
        }
    }

    /// <summary>
    /// Get current subscription for authenticated tenant
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<CurrentSubscriptionDto>> GetCurrentSubscription()
    {
        try
        {
            var tenantId = GetTenantId();
            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(tenantId);
            
            if (subscription == null)
                return NotFound(new { message = "No active subscription found" });

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription");
            return StatusCode(500, new { message = "An error occurred while retrieving the subscription" });
        }
    }

    /// <summary>
    /// Create a trial subscription for a tenant
    /// </summary>
    [HttpPost("trial")]
    public async Task<ActionResult<CurrentSubscriptionDto>> CreateTrialSubscription(
        [FromBody] CreateTrialSubscriptionRequest request)
    {
        try
        {
            request.TenantId = GetTenantId();
            var subscription = await _subscriptionService.CreateTrialSubscriptionAsync(request);
            
            return CreatedAtAction(
                nameof(GetCurrentSubscription),
                new { },
                subscription);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trial subscription");
            return StatusCode(500, new { message = "An error occurred while creating the trial subscription" });
        }
    }

    /// <summary>
    /// Upgrade current subscription to a new plan
    /// </summary>
    [HttpPost("upgrade")]
    public async Task<ActionResult<CurrentSubscriptionDto>> UpgradeSubscription(
        [FromBody] UpgradeSubscriptionRequest request)
    {
        try
        {
            request.TenantId = GetTenantId();
            var subscription = await _subscriptionService.UpgradeSubscriptionAsync(request);
            
            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading subscription");
            return StatusCode(500, new { message = "An error occurred while upgrading the subscription" });
        }
    }

    /// <summary>
    /// Downgrade current subscription to a new plan
    /// </summary>
    [HttpPost("downgrade")]
    public async Task<ActionResult<CurrentSubscriptionDto>> DowngradeSubscription(
        [FromBody] DowngradeSubscriptionRequest request)
    {
        try
        {
            request.TenantId = GetTenantId();
            var subscription = await _subscriptionService.DowngradeSubscriptionAsync(request);
            
            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downgrading subscription");
            return StatusCode(500, new { message = "An error occurred while downgrading the subscription" });
        }
    }

    /// <summary>
    /// Cancel current subscription
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription(
        [FromBody] CancelSubscriptionRequest request)
    {
        try
        {
            request.TenantId = GetTenantId();
            await _subscriptionService.CancelSubscriptionAsync(request);
            
            return Ok(new { message = "Subscription cancelled successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription");
            return StatusCode(500, new { message = "An error occurred while cancelling the subscription" });
        }
    }

    /// <summary>
    /// Get feature limits for current tenant
    /// </summary>
    [HttpGet("feature-limits")]
    public async Task<ActionResult<FeatureLimitsDto>> GetFeatureLimits()
    {
        try
        {
            var tenantId = GetTenantId();
            var limits = await _subscriptionService.GetFeatureLimitsAsync(tenantId);
            
            return Ok(limits);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature limits");
            return StatusCode(500, new { message = "An error occurred while retrieving feature limits" });
        }
    }

    /// <summary>
    /// Check if specific feature is available (within limits)
    /// </summary>
    [HttpGet("feature-limits/{featureName}")]
    public async Task<ActionResult<bool>> CheckFeatureLimit(string featureName)
    {
        try
        {
            var tenantId = GetTenantId();
            var canUse = await _subscriptionService.CheckFeatureLimitAsync(tenantId, featureName);
            
            return Ok(new { feature = featureName, available = canUse });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature limit for {FeatureName}", featureName);
            return StatusCode(500, new { message = "An error occurred while checking feature limit" });
        }
    }

    /// <summary>
    /// Get billing history (invoices) for current tenant
    /// </summary>
    [HttpGet("billing-history")]
    public async Task<IActionResult> GetBillingHistory()
    {
        try
        {
            var tenantId = GetTenantId();
            var invoices = await _invoiceRepository.GetByTenantIdAsync(tenantId);
            
            var result = invoices.Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.InvoiceDate,
                i.DueDate,
                i.TotalAmount,
                i.Currency,
                i.Status,
                i.PaidAt,
                LineItems = i.LineItems.Select(li => new
                {
                    li.Description,
                    li.Quantity,
                    li.UnitPrice,
                    li.LineTotal
                })
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history");
            return StatusCode(500, new { message = "An error occurred while retrieving billing history" });
        }
    }

    /// <summary>
    /// Get specific invoice by ID
    /// </summary>
    [HttpGet("invoices/{invoiceId}")]
    public async Task<IActionResult> GetInvoice(Guid invoiceId)
    {
        try
        {
            var tenantId = GetTenantId();
            var invoice = await _invoiceRepository.GetByIdWithLineItemsAsync(invoiceId);
            
            if (invoice == null)
                return NotFound(new { message = "Invoice not found" });

            // Verify invoice belongs to tenant
            if (invoice.TenantId != tenantId)
                return Forbid();

            var result = new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.InvoiceDate,
                invoice.DueDate,
                invoice.SubTotal,
                invoice.TaxAmount,
                invoice.TotalAmount,
                invoice.Currency,
                invoice.Status,
                invoice.PaidAt,
                invoice.PaymentTransactionId,
                invoice.PdfUrl,
                LineItems = invoice.LineItems.Select(li => new
                {
                    li.Description,
                    li.Quantity,
                    li.UnitPrice,
                    li.LineTotal
                })
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
            return StatusCode(500, new { message = "An error occurred while retrieving the invoice" });
        }
    }

    /// <summary>
    /// Calculate proration for upgrade/downgrade
    /// </summary>
    [HttpPost("calculate-proration")]
    public async Task<ActionResult<ProrationCalculationDto>> CalculateProration(
        [FromBody] Guid newPlanId)
    {
        try
        {
            var tenantId = GetTenantId();
            var proration = await _subscriptionService.CalculateProrationAsync(tenantId, newPlanId);
            
            return Ok(proration);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating proration");
            return StatusCode(500, new { message = "An error occurred while calculating proration" });
        }
    }

    /// <summary>
    /// Update payment method for subscription
    /// </summary>
    [HttpPost("payment-method")]
    public async Task<IActionResult> UpdatePaymentMethod([FromBody] string paymentMethodId)
    {
        try
        {
            var tenantId = GetTenantId();
            
            // TODO: Implement payment method update logic
            // This will integrate with payment gateway (PayTR, Iyzico, etc.)
            
            return Ok(new { message = "Payment method updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method");
            return StatusCode(500, new { message = "An error occurred while updating payment method" });
        }
    }

    /// <summary>
    /// Get current usage statistics
    /// </summary>
    [HttpGet("usage")]
    public async Task<ActionResult<FeatureLimitsDto>> GetUsageStats()
    {
        try
        {
            var tenantId = GetTenantId();
            var limits = await _subscriptionService.GetFeatureLimitsAsync(tenantId);
            
            var usage = new
            {
                staff = new
                {
                    current = limits.CurrentStaffCount,
                    max = limits.MaxStaff,
                    percentage = limits.MaxStaff > 0 ? (limits.CurrentStaffCount * 100.0 / limits.MaxStaff) : 0,
                    canAdd = limits.CanAddStaff
                },
                appointments = new
                {
                    current = limits.CurrentAppointmentsThisMonth,
                    max = limits.MaxAppointmentsPerMonth,
                    percentage = limits.MaxAppointmentsPerMonth > 0 ? (limits.CurrentAppointmentsThisMonth * 100.0 / limits.MaxAppointmentsPerMonth) : 0,
                    canAdd = limits.CanAddAppointment
                },
                locations = new
                {
                    current = limits.CurrentLocationsCount,
                    max = limits.MaxLocations,
                    percentage = limits.MaxLocations > 0 ? (limits.CurrentLocationsCount * 100.0 / limits.MaxLocations) : 0,
                    canAdd = limits.CanAddLocation
                },
                services = new
                {
                    current = limits.CurrentServicesCount,
                    max = limits.MaxServices,
                    percentage = limits.MaxServices > 0 ? (limits.CurrentServicesCount * 100.0 / limits.MaxServices) : 0,
                    canAdd = limits.CanAddService
                }
            };

            return Ok(usage);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats");
            return StatusCode(500, new { message = "An error occurred while retrieving usage statistics" });
        }
    }

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID not found in token");
        }
        return tenantId;
    }
}
