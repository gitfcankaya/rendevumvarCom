using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendevumVar.Application.DTOs;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Infrastructure.Data;
using System.Text.Json;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubscriptionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetSubscriptionPlans()
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        var planDtos = plans.Select(p => new SubscriptionPlanDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            DurationMonths = p.DurationMonths,
            HasFreeTrial = p.HasFreeTrial,
            FreeTrialDays = p.FreeTrialDays,
            IsActive = p.IsActive,
            IsPopular = p.IsPopular,
            SortOrder = p.SortOrder,
            Badge = p.Badge,
            Color = p.Color,
            Features = !string.IsNullOrEmpty(p.Features) ? JsonSerializer.Deserialize<List<string>>(p.Features) : new List<string>()
        });

        return Ok(planDtos);
    }

    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<SubscriptionPlanDto>> GetSubscriptionPlan(Guid id)
    {
        var plan = await _context.SubscriptionPlans
            .Where(p => p.Id == id && p.IsActive)
            .FirstOrDefaultAsync();

        if (plan == null)
            return NotFound();

        var planDto = new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            OriginalPrice = plan.OriginalPrice,
            DurationMonths = plan.DurationMonths,
            HasFreeTrial = plan.HasFreeTrial,
            FreeTrialDays = plan.FreeTrialDays,
            IsActive = plan.IsActive,
            IsPopular = plan.IsPopular,
            SortOrder = plan.SortOrder,
            Badge = plan.Badge,
            Color = plan.Color,
            Features = !string.IsNullOrEmpty(plan.Features) ? JsonSerializer.Deserialize<List<string>>(plan.Features) : new List<string>()
        };

        return Ok(planDto);
    }

    [HttpPost("plans")]
    public async Task<ActionResult<SubscriptionPlanDto>> CreateSubscriptionPlan(CreateSubscriptionPlanDto dto)
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            OriginalPrice = dto.OriginalPrice,
            DurationMonths = dto.DurationMonths,
            HasFreeTrial = dto.HasFreeTrial,
            FreeTrialDays = dto.FreeTrialDays,
            IsActive = dto.IsActive,
            IsPopular = dto.IsPopular,
            SortOrder = dto.SortOrder,
            Badge = dto.Badge,
            Color = dto.Color,
            Features = JsonSerializer.Serialize(dto.Features),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();

        var result = new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            OriginalPrice = plan.OriginalPrice,
            DurationMonths = plan.DurationMonths,
            HasFreeTrial = plan.HasFreeTrial,
            FreeTrialDays = plan.FreeTrialDays,
            IsActive = plan.IsActive,
            IsPopular = plan.IsPopular,
            SortOrder = plan.SortOrder,
            Badge = plan.Badge,
            Color = plan.Color,
            Features = dto.Features
        };

        return CreatedAtAction(nameof(GetSubscriptionPlan), new { id = plan.Id }, result);
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, CreateSubscriptionPlanDto dto)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null)
            return NotFound();

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.Price = dto.Price;
        plan.OriginalPrice = dto.OriginalPrice;
        plan.DurationMonths = dto.DurationMonths;
        plan.HasFreeTrial = dto.HasFreeTrial;
        plan.FreeTrialDays = dto.FreeTrialDays;
        plan.IsActive = dto.IsActive;
        plan.IsPopular = dto.IsPopular;
        plan.SortOrder = dto.SortOrder;
        plan.Badge = dto.Badge;
        plan.Color = dto.Color;
        plan.Features = JsonSerializer.Serialize(dto.Features);
        plan.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SubscriptionPlanExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("plans/{id:guid}")]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(id);
        if (plan == null)
            return NotFound();

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetSubscription(Guid id)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();

        if (subscription == null)
            return NotFound();

        var subscriptionDto = new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = subscription.SubscriptionPlan.Name,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive,
            IsFreeTrial = subscription.IsFreeTrial,
            AmountPaid = subscription.AmountPaid,
            CancelledAt = subscription.CancelledAt,
            CancellationReason = subscription.CancellationReason,
            Status = subscription.Status
        };

        return Ok(subscriptionDto);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetUserSubscriptions(Guid userId)
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        var subscriptionDtos = subscriptions.Select(s => new SubscriptionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            SubscriptionPlanId = s.SubscriptionPlanId,
            PlanName = s.SubscriptionPlan.Name,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            IsActive = s.IsActive,
            IsFreeTrial = s.IsFreeTrial,
            AmountPaid = s.AmountPaid,
            CancelledAt = s.CancelledAt,
            CancellationReason = s.CancellationReason,
            Status = s.Status
        });

        return Ok(subscriptionDtos);
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto dto)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(dto.SubscriptionPlanId);
        if (plan == null)
            return BadRequest("Subscription plan not found");

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(plan.DurationMonths),
            IsActive = true,
            IsFreeTrial = dto.IsFreeTrial,
            AmountPaid = dto.IsFreeTrial ? 0 : plan.Price,
            Status = dto.IsFreeTrial ? SubscriptionStatus.FreeTrial : SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.IsFreeTrial && plan.HasFreeTrial)
        {
            subscription.EndDate = DateTime.UtcNow.AddDays(plan.FreeTrialDays);
        }

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var result = new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            PlanName = plan.Name,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            IsActive = subscription.IsActive,
            IsFreeTrial = subscription.IsFreeTrial,
            AmountPaid = subscription.AmountPaid,
            Status = subscription.Status
        };

        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] string? reason = null)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
            return NotFound();

        subscription.IsActive = false;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = reason;
        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("process-payment")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment(CreateSubscriptionPaymentDto dto)
    {
        // Simulate PayTR payment processing
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            SubscriptionId = dto.SubscriptionId,
            Amount = dto.Amount,
            Currency = "TRY",
            Method = dto.Method,
            Status = PaymentStatus.Pending,
            PaymentGateway = "PayTR",
            PaymentReference = dto.PaymentReference ?? Guid.NewGuid().ToString(),
            TransactionId = $"PAYTR_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(1000, 9999)}",
            CreatedAt = DateTime.UtcNow
        };

        // Simulate payment success/failure (90% success rate)
        var isSuccessful = Random.Shared.NextDouble() > 0.1;

        if (isSuccessful)
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaymentDate = DateTime.UtcNow;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Payment declined by bank";
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var result = new PaymentDto
        {
            Id = payment.Id,
            UserId = payment.UserId,
            SubscriptionId = payment.SubscriptionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Method = payment.Method,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaymentGateway = payment.PaymentGateway,
            PaymentReference = payment.PaymentReference,
            FailureReason = payment.FailureReason,
            PaymentDate = payment.PaymentDate,
            CreatedAt = payment.CreatedAt
        };

        return Ok(result);
    }

    private bool SubscriptionPlanExists(Guid id)
    {
        return _context.SubscriptionPlans.Any(e => e.Id == id);
    }
}