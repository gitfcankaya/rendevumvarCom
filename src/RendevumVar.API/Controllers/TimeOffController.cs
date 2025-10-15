using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.API.Authorization;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Services;
using RendevumVar.Core.Constants;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeOffController : ControllerBase
{
    private readonly ITimeOffService _timeOffService;
    private readonly ILogger<TimeOffController> _logger;

    public TimeOffController(ITimeOffService timeOffService, ILogger<TimeOffController> logger)
    {
        _timeOffService = timeOffService;
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
    /// Request time off
    /// </summary>
    [HttpPost("request")]
    [HasPermission(Permissions.RequestTimeOff)]
    public async Task<ActionResult<TimeOffRequestDto>> RequestTimeOff([FromBody] CreateTimeOffRequestDto dto)
    {
        try
        {
            // TODO: Get staffId from current user's context
            var staffId = GetUserId(); // Temporary - should map to staff record
            var result = await _timeOffService.RequestTimeOffAsync(staffId, dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error requesting time off");
            return StatusCode(500, new { error = "An error occurred while requesting time off" });
        }
    }

    /// <summary>
    /// Get time off requests for a specific staff member
    /// </summary>
    [HttpGet("staff/{staffId}")]
    [HasPermission(Permissions.ViewTimeOff)]
    public async Task<ActionResult<IEnumerable<TimeOffRequestDto>>> GetStaffTimeOff(Guid staffId)
    {
        try
        {
            var result = await _timeOffService.GetStaffTimeOffAsync(staffId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff time off");
            return StatusCode(500, new { error = "An error occurred while getting staff time off" });
        }
    }

    /// <summary>
    /// Get pending time off requests for the tenant (for managers)
    /// </summary>
    [HttpGet("pending")]
    [HasPermission(Permissions.ManageTimeOff)]
    public async Task<ActionResult<IEnumerable<TimeOffRequestDto>>> GetPendingRequests()
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _timeOffService.GetPendingRequestsAsync(tenantId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending requests");
            return StatusCode(500, new { error = "An error occurred while getting pending requests" });
        }
    }

    /// <summary>
    /// Approve time off request
    /// </summary>
    [HttpPost("{requestId}/approve")]
    [HasPermission(Permissions.ApproveTimeOff)]
    public async Task<ActionResult<TimeOffRequestDto>> ApproveTimeOffRequest(Guid requestId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _timeOffService.ApproveTimeOffRequestAsync(requestId, userId);
            return Ok(result);
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
            _logger.LogError(ex, "Error approving time off request");
            return StatusCode(500, new { error = "An error occurred while approving time off request" });
        }
    }

    /// <summary>
    /// Reject time off request
    /// </summary>
    [HttpPost("{requestId}/reject")]
    [HasPermission(Permissions.RejectTimeOff)]
    public async Task<ActionResult<TimeOffRequestDto>> RejectTimeOffRequest(Guid requestId, [FromBody] RejectTimeOffDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _timeOffService.RejectTimeOffRequestAsync(requestId, userId, dto.Reason);
            return Ok(result);
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
            _logger.LogError(ex, "Error rejecting time off request");
            return StatusCode(500, new { error = "An error occurred while rejecting time off request" });
        }
    }

    /// <summary>
    /// Cancel time off request
    /// </summary>
    [HttpPost("{requestId}/cancel")]
    [HasPermission(Permissions.RequestTimeOff)]
    public async Task<IActionResult> CancelTimeOffRequest(Guid requestId)
    {
        try
        {
            await _timeOffService.CancelTimeOffRequestAsync(requestId);
            return Ok(new { message = "Time off request cancelled successfully" });
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
            _logger.LogError(ex, "Error cancelling time off request");
            return StatusCode(500, new { error = "An error occurred while cancelling time off request" });
        }
    }
}

// Helper DTO for reject
public class RejectTimeOffDto
{
    public required string Reason { get; set; }
}
