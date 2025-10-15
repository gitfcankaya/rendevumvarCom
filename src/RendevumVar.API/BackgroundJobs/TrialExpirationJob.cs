using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.API.BackgroundJobs;

/// <summary>
/// Background job to check and expire trial subscriptions daily
/// Runs at 02:00 AM UTC every day
/// </summary>
public class TrialExpirationJob : BackgroundService
{
    private readonly ILogger<TrialExpirationJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run daily

    public TrialExpirationJob(
        ILogger<TrialExpirationJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trial Expiration Job started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                
                // Calculate delay until next 2 AM UTC
                var next2AM = now.Date.AddDays(1).AddHours(2);
                if (now.Hour < 2)
                {
                    next2AM = now.Date.AddHours(2);
                }
                
                var delay = next2AM - now;
                
                _logger.LogInformation("Next trial expiration check scheduled at {Next2AM} UTC (in {Delay})", 
                    next2AM, delay);
                
                await Task.Delay(delay, stoppingToken);
                
                await CheckAndExpireTrialsAsync();
            }
            catch (OperationCanceledException)
            {
                // Normal when stopping the service
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Trial Expiration Job");
                // Wait before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Trial Expiration Job stopped");
    }

    private async Task CheckAndExpireTrialsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var subscriptionRepository = scope.ServiceProvider.GetRequiredService<ITenantSubscriptionRepository>();

        _logger.LogInformation("Checking for expiring trials...");

        var tomorrow = DateTime.UtcNow.AddDays(1);
        var expiringTrials = await subscriptionRepository.GetExpiringTrialsAsync(tomorrow);

        var trialsList = expiringTrials.ToList();
        _logger.LogInformation("Found {Count} expiring trials", trialsList.Count);

        foreach (var subscription in trialsList)
        {
            try
            {
                if (!subscription.TrialEndDate.HasValue)
                    continue;

                var daysUntilExpiry = (subscription.TrialEndDate.Value - DateTime.UtcNow).Days;

                if (daysUntilExpiry <= 0)
                {
                    // Trial has expired
                    subscription.Status = SubscriptionStatus.Expired;
                    subscription.AutoRenew = false;
                    subscription.UpdatedAt = DateTime.UtcNow;
                    subscription.UpdatedBy = "TrialExpirationJob";

                    await subscriptionRepository.UpdateAsync(subscription);

                    _logger.LogInformation(
                        "Trial subscription {SubscriptionId} for tenant {TenantId} has been expired",
                        subscription.Id, subscription.TenantId);

                    // TODO: Send expiration notification email
                }
                else if (daysUntilExpiry <= 3)
                {
                    // Send reminder - trial expiring soon
                    _logger.LogInformation(
                        "Trial subscription {SubscriptionId} for tenant {TenantId} expires in {Days} days",
                        subscription.Id, subscription.TenantId, daysUntilExpiry);

                    // TODO: Send reminder notification email
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing trial expiration for subscription {SubscriptionId}",
                    subscription.Id);
            }
        }
    }
}
