using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.API.BackgroundJobs;

/// <summary>
/// Background job to check overdue invoices and update subscription status
/// Runs daily at 03:00 AM UTC
/// </summary>
public class OverdueInvoiceJob : BackgroundService
{
    private readonly ILogger<OverdueInvoiceJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OverdueInvoiceJob(
        ILogger<OverdueInvoiceJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Overdue Invoice Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Calculate delay until next 3 AM UTC
                var next3AM = now.Date.AddDays(1).AddHours(3);
                if (now.Hour < 3)
                {
                    next3AM = now.Date.AddHours(3);
                }
                
                var delay = next3AM - now;
                
                _logger.LogInformation("Next overdue invoice check scheduled at {Next3AM} UTC (in {Delay})", 
                    next3AM, delay);
                
                await Task.Delay(delay, stoppingToken);
                
                await CheckOverdueInvoicesAsync();
            }
            catch (OperationCanceledException)
            {
                // Normal when stopping the service
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Overdue Invoice Job");
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Overdue Invoice Job stopped");
    }

    private async Task CheckOverdueInvoicesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ITenantSubscriptionRepository>();

        _logger.LogInformation("Checking for overdue invoices...");

        var overdueInvoices = await invoiceRepository.GetOverdueInvoicesAsync();

        var invoicesList = overdueInvoices.ToList();
        _logger.LogInformation("Found {Count} overdue invoices", invoicesList.Count);

        foreach (var invoice in invoicesList)
        {
            try
            {
                // Update invoice status to overdue
                invoice.Status = InvoiceStatus.Overdue;
                invoice.UpdatedAt = DateTime.UtcNow;
                invoice.UpdatedBy = "OverdueInvoiceJob";

                await invoiceRepository.UpdateAsync(invoice);

                _logger.LogInformation(
                    "Invoice {InvoiceNumber} marked as overdue for tenant {TenantId}",
                    invoice.InvoiceNumber, invoice.TenantId);

                // Check if subscription should be suspended
                var subscription = await subscriptionRepository.GetCurrentSubscriptionByTenantIdAsync(invoice.TenantId);
                
                if (subscription != null && subscription.Status == SubscriptionStatus.Active)
                {
                    var daysOverdue = (DateTime.UtcNow - invoice.DueDate).Days;

                    if (daysOverdue > 7) // Suspend after 7 days overdue
                    {
                        subscription.Status = SubscriptionStatus.PastDue;
                        subscription.UpdatedAt = DateTime.UtcNow;
                        subscription.UpdatedBy = "OverdueInvoiceJob";

                        await subscriptionRepository.UpdateAsync(subscription);

                        _logger.LogWarning(
                            "Subscription {SubscriptionId} for tenant {TenantId} marked as PastDue due to overdue invoice",
                            subscription.Id, subscription.TenantId);

                        // TODO: Send past due notification email
                    }
                    else if (daysOverdue > 0)
                    {
                        // Send reminder
                        _logger.LogInformation(
                            "Invoice {InvoiceNumber} is {Days} days overdue for tenant {TenantId}",
                            invoice.InvoiceNumber, daysOverdue, invoice.TenantId);

                        // TODO: Send overdue reminder email
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing overdue invoice {InvoiceNumber}",
                    invoice.InvoiceNumber);
            }
        }
    }
}
