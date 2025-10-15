using System.Security.Claims;
using RendevumVar.Application.Services;

namespace RendevumVar.API.Middleware;

/// <summary>
/// Middleware to enforce subscription feature limits
/// Checks if tenant has reached their plan limits before processing requests
/// </summary>
public class SubscriptionEnforcementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SubscriptionEnforcementMiddleware> _logger;

    // Endpoints that require feature limit checks
    private static readonly Dictionary<string, string> FeatureLimitEndpoints = new()
    {
        { "POST:/api/staff", "staff" },
        { "POST:/api/appointments", "appointment" },
        { "POST:/api/salons", "location" },
        { "POST:/api/services", "service" }
    };

    public SubscriptionEnforcementMiddleware(
        RequestDelegate next,
        ILogger<SubscriptionEnforcementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISubscriptionService subscriptionService)
    {
        // Skip for non-authenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Skip for GET requests and subscription endpoints
        if (context.Request.Method == "GET" || 
            context.Request.Path.StartsWithSegments("/api/subscriptions"))
        {
            await _next(context);
            return;
        }

        // Check if this endpoint requires feature limit check
        var endpoint = $"{context.Request.Method}:{context.Request.Path.Value}";
        var featureName = GetFeatureNameForEndpoint(endpoint);

        if (!string.IsNullOrEmpty(featureName))
        {
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            
            if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                _logger.LogWarning("Tenant ID not found in token");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Tenant ID not found" });
                return;
            }

            try
            {
                var canUseFeature = await subscriptionService.CheckFeatureLimitAsync(tenantId, featureName);

                if (!canUseFeature)
                {
                    _logger.LogWarning(
                        "Feature limit exceeded for tenant {TenantId} on feature {FeatureName}",
                        tenantId, featureName);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = $"Feature limit exceeded: You have reached the maximum number of {featureName}s allowed in your subscription plan",
                        feature = featureName,
                        upgradeRequired = true
                    });
                    return;
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "No active subscription found for tenant {TenantId}", tenantId);
                
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "No active subscription found. Please subscribe to continue using the service.",
                    subscriptionRequired = true
                });
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking feature limits for tenant {TenantId}", tenantId);
                // Continue processing - don't block on middleware errors
            }
        }

        await _next(context);
    }

    private static string? GetFeatureNameForEndpoint(string endpoint)
    {
        // Direct match
        if (FeatureLimitEndpoints.TryGetValue(endpoint, out var feature))
            return feature;

        // Pattern matching for dynamic routes
        foreach (var kvp in FeatureLimitEndpoints)
        {
            if (endpoint.StartsWith(kvp.Key.Split(':')[1].TrimEnd('/')))
                return kvp.Value;
        }

        return null;
    }
}

/// <summary>
/// Extension method to register subscription enforcement middleware
/// </summary>
public static class SubscriptionEnforcementMiddlewareExtensions
{
    public static IApplicationBuilder UseSubscriptionEnforcement(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SubscriptionEnforcementMiddleware>();
    }
}
