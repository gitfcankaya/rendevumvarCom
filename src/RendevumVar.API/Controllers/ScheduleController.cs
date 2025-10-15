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
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<ScheduleController> _logger;

    public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    /// <summary>
    /// Set/Create staff schedule
    /// </summary>
    [HttpPost("staff/{staffId}")]
    [HasPermission(Permissions.CreateSchedules)]
    public async Task<ActionResult<StaffScheduleDto>> SetStaffSchedule(Guid staffId, [FromBody] SetStaffScheduleDto dto)
    {
        try
        {
            var result = await _scheduleService.SetStaffScheduleAsync(staffId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting staff schedule");
            return StatusCode(500, new { error = "An error occurred while setting staff schedule" });
        }
    }

    /// <summary>
    /// Get all schedules for a staff member
    /// </summary>
    [HttpGet("staff/{staffId}")]
    [HasPermission(Permissions.ViewSchedules)]
    public async Task<ActionResult<IEnumerable<StaffScheduleDto>>> GetStaffSchedule(Guid staffId)
    {
        try
        {
            var result = await _scheduleService.GetStaffScheduleAsync(staffId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff schedule");
            return StatusCode(500, new { error = "An error occurred while getting staff schedule" });
        }
    }

    /// <summary>
    /// Update existing staff schedule
    /// </summary>
    [HttpPut("{scheduleId}")]
    [HasPermission(Permissions.EditSchedules)]
    public async Task<ActionResult<StaffScheduleDto>> UpdateStaffSchedule(Guid scheduleId, [FromBody] SetStaffScheduleDto dto)
    {
        try
        {
            var result = await _scheduleService.UpdateStaffScheduleAsync(scheduleId, dto);
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
            _logger.LogError(ex, "Error updating staff schedule");
            return StatusCode(500, new { error = "An error occurred while updating staff schedule" });
        }
    }

    /// <summary>
    /// Delete (deactivate) staff schedule
    /// </summary>
    [HttpDelete("{scheduleId}")]
    [HasPermission(Permissions.DeleteSchedules)]
    public async Task<IActionResult> DeleteStaffSchedule(Guid scheduleId)
    {
        try
        {
            await _scheduleService.DeleteStaffScheduleAsync(scheduleId);
            return Ok(new { message = "Schedule deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff schedule");
            return StatusCode(500, new { error = "An error occurred while deleting staff schedule" });
        }
    }

    /// <summary>
    /// Check if staff is available at a specific time
    /// </summary>
    [HttpPost("availability/check")]
    [HasPermission(Permissions.ViewSchedules)]
    public async Task<ActionResult<bool>> CheckStaffAvailability([FromBody] CheckAvailabilityDto dto)
    {
        try
        {
            var result = await _scheduleService.CheckStaffAvailabilityAsync(
                dto.StaffId,
                dto.DateTime,
                dto.DurationMinutes);
            return Ok(new { available = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking staff availability");
            return StatusCode(500, new { error = "An error occurred while checking staff availability" });
        }
    }

    /// <summary>
    /// Get available time slots for a staff member on a specific date
    /// </summary>
    [HttpGet("availability/slots")]
    [HasPermission(Permissions.ViewSchedules)]
    public async Task<ActionResult<IEnumerable<AvailableTimeSlotDto>>> GetAvailableTimeSlots(
        [FromQuery] Guid staffId,
        [FromQuery] DateTime date,
        [FromQuery] int slotDurationMinutes = 30)
    {
        try
        {
            var result = await _scheduleService.GetAvailableTimeSlotsAsync(staffId, date, slotDurationMinutes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available time slots");
            return StatusCode(500, new { error = "An error occurred while getting available time slots" });
        }
    }
}

// Helper DTO for availability check
public class CheckAvailabilityDto
{
    public Guid StaffId { get; set; }
    public DateTime DateTime { get; set; }
    public int DurationMinutes { get; set; }
}
