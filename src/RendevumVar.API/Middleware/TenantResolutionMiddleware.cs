using Microsoft.EntityFrameworkCore;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.API.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolutionMiddleware> _logger;

        public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            try
            {
                // Extract tenant information from request
                var tenantId = await ResolveTenantAsync(context, dbContext);
                
                if (tenantId.HasValue)
                {
                    // Store tenant ID in HTTP context
                    context.Items["TenantId"] = tenantId.Value;
                    _logger.LogInformation("Tenant resolved: {TenantId}", tenantId.Value);
                }
                else
                {
                    _logger.LogWarning("No tenant could be resolved for request: {Path}", context.Request.Path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant for request: {Path}", context.Request.Path);
            }

            await _next(context);
        }

        private async Task<Guid?> ResolveTenantAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Strategy 1: Extract from JWT token
            var tokenTenantId = ExtractTenantFromToken(context);
            if (tokenTenantId.HasValue)
            {
                return tokenTenantId;
            }

            // Strategy 2: Extract from subdomain
            var subdomainTenantId = await ExtractTenantFromSubdomainAsync(context, dbContext);
            if (subdomainTenantId.HasValue)
            {
                return subdomainTenantId;
            }

            // Strategy 3: Extract from custom header
            var headerTenantId = ExtractTenantFromHeader(context);
            if (headerTenantId.HasValue)
            {
                return headerTenantId;
            }

            // Strategy 4: Default tenant for unauthenticated requests
            return await GetDefaultTenantAsync(dbContext);
        }

        private Guid? ExtractTenantFromToken(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst("tenant_id");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
                {
                    return tenantId;
                }
            }
            return null;
        }

        private async Task<Guid?> ExtractTenantFromSubdomainAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var host = context.Request.Host.Host;
            
            // Extract subdomain (assuming format: subdomain.domain.com)
            var parts = host.Split('.');
            if (parts.Length >= 3)
            {
                var subdomain = parts[0];
                
                // Skip common subdomains
                if (subdomain == "www" || subdomain == "api")
                {
                    return null;
                }

                var tenant = await dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.Status == "Active");
                
                return tenant?.Id;
            }

            return null;
        }

        private Guid? ExtractTenantFromHeader(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader))
            {
                if (Guid.TryParse(tenantHeader.FirstOrDefault(), out var tenantId))
                {
                    return tenantId;
                }
            }
            return null;
        }

        private async Task<Guid?> GetDefaultTenantAsync(ApplicationDbContext dbContext)
        {
            // Return the first active tenant as default, or null
            var defaultTenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Status == "Active");
            
            return defaultTenant?.Id;
        }
    }

    public static class TenantResolutionMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantResolutionMiddleware>();
        }
    }
}