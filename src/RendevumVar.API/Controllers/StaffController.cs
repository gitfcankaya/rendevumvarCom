using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Services;
using RendevumVar.API.Authorization;
using RendevumVar.Core.Constants;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _logger = logger;
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

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }

    /// <summary>
    /// Invite a new staff member
    /// </summary>
    [HttpPost("invite")]
    [HasPermission(Permissions.InviteStaff)]
    public async Task<ActionResult<StaffDto>> InviteStaff([FromBody] InviteStaffDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _staffService.InviteStaffAsync(tenantId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting staff");
            return StatusCode(500, new { error = "An error occurred while inviting staff" });
        }
    }

    /// <summary>
    /// Accept staff invitation (public endpoint, no auth required)
    /// </summary>
    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    public async Task<ActionResult<StaffDto>> AcceptInvitation([FromBody] AcceptInvitationDto dto)
    {
        try
        {
            var result = await _staffService.AcceptInvitationAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation");
            return StatusCode(500, new { error = "An error occurred while accepting invitation" });
        }
    }

    /// <summary>
    /// Resend staff invitation
    /// </summary>
    [HttpPost("{staffId}/resend-invitation")]
    [HasPermission(Permissions.InviteStaff)]
    public async Task<IActionResult> ResendInvitation(Guid staffId)
    {
        try
        {
            await _staffService.ResendInvitationAsync(staffId);
            return Ok(new { message = "Invitation resent successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation");
            return StatusCode(500, new { error = "An error occurred while resending invitation" });
        }
    }

    /// <summary>
    /// Get all staff members for the tenant
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.ViewStaff)]
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetStaffList()
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _staffService.GetStaffListAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff list");
            return StatusCode(500, new { error = "An error occurred while getting staff list" });
        }
    }

    /// <summary>
    /// Get staff member details by ID
    /// </summary>
    [HttpGet("{staffId}")]
    [HasPermission(Permissions.ViewStaff)]
    public async Task<ActionResult<StaffDto>> GetStaffDetails(Guid staffId)
    {
        try
        {
            var result = await _staffService.GetStaffDetailsAsync(staffId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff details");
            return StatusCode(500, new { error = "An error occurred while getting staff details" });
        }
    }

    /// <summary>
    /// Update staff profile
    /// </summary>
    [HttpPut("{staffId}")]
    [HasPermission(Permissions.EditStaff)]
    public async Task<ActionResult<StaffDto>> UpdateStaffProfile(Guid staffId, [FromBody] UpdateStaffProfileDto dto)
    {
        try
        {
            var result = await _staffService.UpdateStaffProfileAsync(staffId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff profile");
            return StatusCode(500, new { error = "An error occurred while updating staff profile" });
        }
    }

    /// <summary>
    /// Deactivate staff member
    /// </summary>
    [HttpPost("{staffId}/deactivate")]
    [HasPermission(Permissions.ManageStaff)]
    public async Task<IActionResult> DeactivateStaff(Guid staffId)
    {
        try
        {
            await _staffService.DeactivateStaffAsync(staffId);
            return Ok(new { message = "Staff deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating staff");
            return StatusCode(500, new { error = "An error occurred while deactivating staff" });
        }
    }

    /// <summary>
    /// Reactivate staff member
    /// </summary>
    [HttpPost("{staffId}/reactivate")]
    [HasPermission(Permissions.ManageStaff)]
    public async Task<IActionResult> ReactivateStaff(Guid staffId)
    {
        try
        {
            await _staffService.ReactivateStaffAsync(staffId);
            return Ok(new { message = "Staff reactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating staff");
            return StatusCode(500, new { error = "An error occurred while reactivating staff" });
        }
    }

    /// <summary>
    /// Assign role to staff member
    /// </summary>
    [HttpPost("{staffId}/assign-role")]
    [HasPermission(Permissions.ManageStaff)]
    public async Task<IActionResult> AssignRole(Guid staffId, [FromBody] AssignRoleDto dto)
    {
        try
        {
            await _staffService.AssignRoleAsync(staffId, dto.RoleId);
            return Ok(new { message = "Role assigned successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role");
            return StatusCode(500, new { error = "An error occurred while assigning role" });
        }
    }

    /// <summary>
    /// Get all roles for the tenant
    /// </summary>
    [HttpGet("roles")]
    [HasPermission(Permissions.ViewRoles)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _staffService.GetRolesAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles");
            return StatusCode(500, new { error = "An error occurred while getting roles" });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(Permissions.CreateRoles)]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _staffService.CreateRoleAsync(tenantId, dto);
            return CreatedAtAction(nameof(GetRoles), new { }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new { error = "An error occurred while creating role" });
        }
    }
}

// Helper DTO for assign role
public class AssignRoleDto
{
    public Guid RoleId { get; set; }
}
