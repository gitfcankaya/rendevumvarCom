using Microsoft.AspNetCore.Authorization;
using RendevumVar.Core.Repositories;
using System.Security.Claims;
using System.Text.Json;

namespace RendevumVar.API.Authorization;

/// <summary>
/// Authorization handler for permission-based access control
/// Checks if user's role contains the required permission
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IServiceProvider serviceProvider,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get user ID from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("User ID not found in claims");
            context.Fail();
            return;
        }

        // Get tenant ID from claims
        var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            _logger.LogWarning("Tenant ID not found in claims for user {UserId}", userId);
            context.Fail();
            return;
        }

        try
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();
            var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

            // Get staff record for the user
            var staff = await staffRepository.GetWithRoleAsync(userId);
            if (staff == null || !staff.RoleId.HasValue)
            {
                _logger.LogWarning("Staff record or role not found for user {UserId}", userId);
                context.Fail();
                return;
            }

            // Get role with permissions
            var role = await roleRepository.GetByIdAsync(staff.RoleId.Value);
            if (role == null)
            {
                _logger.LogWarning("Role {RoleId} not found for staff {StaffId}", staff.RoleId.Value, staff.Id);
                context.Fail();
                return;
            }

            // Parse permissions from JSON
            List<string> permissions;
            try
            {
                permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions) ?? new List<string>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize permissions for role {RoleId}", role.Id);
                context.Fail();
                return;
            }

            // Check if user has the required permission
            if (permissions.Contains(requirement.Permission))
            {
                _logger.LogDebug("User {UserId} has permission {Permission}", userId, requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have permission {Permission}", userId, requirement.Permission);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", requirement.Permission, userId);
            context.Fail();
        }
    }
}
