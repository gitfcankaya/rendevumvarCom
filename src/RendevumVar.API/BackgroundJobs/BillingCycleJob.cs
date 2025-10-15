using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.API.BackgroundJobs;

/// <summary>
/// Background job to process billing cycles and generate invoices
/// Runs daily at 01:00 AM UTC
/// </summary>
public class BillingCycleJob : BackgroundService
{
    private readonly ILogger<BillingCycleJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BillingCycleJob(
        ILogger<BillingCycleJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Billing Cycle Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Calculate delay until next 1 AM UTC
                var next1AM = now.Date.AddDays(1).AddHours(1);
                if (now.Hour < 1)
                {
                    next1AM = now.Date.AddHours(1);
                }
                
                var delay = next1AM - now;
                
                _logger.LogInformation("Next billing cycle check scheduled at {Next1AM} UTC (in {Delay})", 
                    next1AM, delay);
                
                await Task.Delay(delay, stoppingToken);
                
                await ProcessBillingCyclesAsync();
            }
            catch (OperationCanceledException)
            {
                // Normal when stopping the service
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Billing Cycle Job");
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Billing Cycle Job stopped");
    }

    private async Task ProcessBillingCyclesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ITenantSubscriptionRepository>();
        var invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();

        _logger.LogInformation("Processing due billing cycles...");

        var today = DateTime.UtcNow.Date;
        var dueBillings = await subscriptionRepository.GetDueBillingsAsync(today);

        var billingsList = dueBillings.ToList();
        _logger.LogInformation("Found {Count} subscriptions due for billing", billingsList.Count);

        foreach (var subscription in billingsList)
        {
            try
            {
                await GenerateInvoiceAsync(subscription, invoiceRepository);
                
                // Update next billing date
                var billingMonths = subscription.BillingCycle == BillingCycle.Annual ? 12 : 1;
                subscription.NextBillingDate = subscription.NextBillingDate?.AddMonths(billingMonths);
                subscription.UpdatedAt = DateTime.UtcNow;
                subscription.UpdatedBy = "BillingCycleJob";

                await subscriptionRepository.UpdateAsync(subscription);

                _logger.LogInformation(
                    "Processed billing for subscription {SubscriptionId}, next billing: {NextBilling}",
                    subscription.Id, subscription.NextBillingDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing billing for subscription {SubscriptionId}",
                    subscription.Id);
            }
        }
    }

    private async Task GenerateInvoiceAsync(
        TenantSubscription subscription,
        IInvoiceRepository invoiceRepository)
    {
        var plan = subscription.SubscriptionPlan;
        var amount = subscription.BillingCycle == BillingCycle.Annual
            ? plan.AnnualPrice
            : plan.MonthlyPrice;

        var invoiceNumber = await invoiceRepository.GenerateInvoiceNumberAsync();
        var now = DateTime.UtcNow;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            TenantId = subscription.TenantId,
            TenantSubscriptionId = subscription.Id,
            InvoiceDate = now,
            DueDate = now.AddDays(7), // 7 days payment term
            SubTotal = amount,
            TaxAmount = amount * 0.20m, // 20% VAT (adjust based on region)
            TotalAmount = amount * 1.20m,
            Currency = "TRY",
            Status = InvoiceStatus.Sent,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "BillingCycleJob",
            IsDeleted = false
        };

        // Add line item
        var lineItem = new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Description = $"{plan.Name} - {subscription.BillingCycle} Subscription",
            Quantity = 1,
            UnitPrice = amount,
            LineTotal = amount,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "BillingCycleJob",
            IsDeleted = false
        };

        invoice.LineItems = new List<InvoiceLineItem> { lineItem };

        await invoiceRepository.AddAsync(invoice);

        _logger.LogInformation(
            "Generated invoice {InvoiceNumber} for tenant {TenantId}, amount: {Amount} {Currency}",
            invoiceNumber, subscription.TenantId, invoice.TotalAmount, invoice.Currency);

        // TODO: Send invoice email to tenant
        // TODO: Process payment via payment gateway if auto-pay is enabled
    }
}
