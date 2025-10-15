using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Enums;
using System.Security.Claims;

namespace RendevumVar.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAvailabilityService _availabilityService;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IAvailabilityService availabilityService,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _availabilityService = availabilityService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            var customerId = GetUserId();
            var tenantId = GetTenantId();

            var appointment = await _appointmentService.CreateAppointmentAsync(dto, customerId, tenantId);

            return CreatedAtAction(
                nameof(GetAppointmentDetails),
                new { id = appointment.Id },
                appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return StatusCode(500, new { error = "An error occurred while creating the appointment" });
        }
    }

    /// <summary>
    /// Get appointment details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDetailsDto>> GetAppointmentDetails(Guid id)
    {
        try
        {
            var tenantId = GetTenantId();
            var appointment = await _appointmentService.GetAppointmentDetailsAsync(id, tenantId);

            if (appointment == null)
            {
                return NotFound(new { error = "Appointment not found" });
            }

            // Verify user has access to this appointment
            var userId = GetUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (appointment.CustomerId != userId &&
                userRole != "Admin" &&
                userRole != "SalonOwner" &&
                userRole != "Staff")
            {
                return Forbid();
            }

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment details for ID {AppointmentId}", id);
            return StatusCode(500, new { error = "An error occurred while fetching appointment details" });
        }
    }

    /// <summary>
    /// Get customer's own appointments
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMyAppointments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] AppointmentStatus? status = null)
    {
        try
        {
            var customerId = GetUserId();
            var appointments = await _appointmentService.GetCustomerAppointmentsAsync(
                customerId,
                startDate,
                endDate,
                status);

            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer appointments");
            return StatusCode(500, new { error = "An error occurred while fetching appointments" });
        }
    }

    /// <summary>
    /// Get appointments for a specific staff member
    /// </summary>
    [HttpGet("staff/{staffId}")]
    [Authorize(Roles = "Admin,SalonOwner,Staff")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetStaffAppointments(
        Guid staffId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var appointments = await _appointmentService.GetStaffAppointmentsAsync(
                staffId,
                tenantId,
                startDate,
                endDate);

            return Ok(appointments);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff appointments for StaffId {StaffId}", staffId);
            return StatusCode(500, new { error = "An error occurred while fetching staff appointments" });
        }
    }

    /// <summary>
    /// Get appointments for a specific salon
    /// </summary>
    [HttpGet("salon/{salonId}")]
    [Authorize(Roles = "Admin,SalonOwner,Staff")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetSalonAppointments(
        Guid salonId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var appointments = await _appointmentService.GetSalonAppointmentsAsync(
                salonId,
                tenantId,
                startDate,
                endDate);

            return Ok(appointments);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salon appointments for SalonId {SalonId}", salonId);
            return StatusCode(500, new { error = "An error occurred while fetching salon appointments" });
        }
    }

    /// <summary>
    /// Update appointment status
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,SalonOwner,Staff")]
    public async Task<ActionResult<AppointmentDto>> UpdateAppointmentStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var appointment = await _appointmentService.UpdateAppointmentStatusAsync(id, dto, tenantId);

            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appointment status for ID {AppointmentId}", id);
            return StatusCode(500, new { error = "An error occurred while updating appointment status" });
        }
    }

    /// <summary>
    /// Reschedule an appointment
    /// </summary>
    [HttpPut("{id}/reschedule")]
    public async Task<ActionResult<AppointmentDto>> RescheduleAppointment(
        Guid id,
        [FromBody] RescheduleAppointmentDto dto)
    {
        try
        {
            var tenantId = GetTenantId();
            var appointment = await _appointmentService.RescheduleAppointmentAsync(
                id,
                dto.NewStartTime,
                dto.NewStaffId,
                tenantId);

            return Ok(appointment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rescheduling appointment ID {AppointmentId}", id);
            return StatusCode(500, new { error = "An error occurred while rescheduling the appointment" });
        }
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelAppointment(
        Guid id,
        [FromBody] CancelAppointmentDto? dto = null)
    {
        try
        {
            var tenantId = GetTenantId();
            var cancellationReason = dto?.CancellationReason;

            await _appointmentService.CancelAppointmentAsync(id, cancellationReason, tenantId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment ID {AppointmentId}", id);
            return StatusCode(500, new { error = "An error occurred while cancelling the appointment" });
        }
    }

    /// <summary>
    /// Get available time slots for a staff member on a specific date
    /// </summary>
    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AvailableTimeSlotDto>>> GetAvailableTimeSlots(
        [FromQuery] Guid staffId,
        [FromQuery] DateTime date,
        [FromQuery] int serviceDurationMinutes = 60)
    {
        try
        {
            var slots = await _availabilityService.CalculateOptimalSlotsAsync(
                staffId,
                date,
                serviceDurationMinutes);

            return Ok(slots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available time slots for StaffId {StaffId} on {Date}", staffId, date);
            return StatusCode(500, new { error = "An error occurred while fetching available time slots" });
        }
    }

    /// <summary>
    /// Get available staff for a service at a specific time
    /// </summary>
    [HttpGet("availability/staff")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetAvailableStaff(
        [FromQuery] Guid salonId,
        [FromQuery] Guid serviceId,
        [FromQuery] DateTime dateTime,
        [FromQuery] int durationMinutes = 60)
    {
        try
        {
            var tenantId = GetTenantIdOrDefault();
            var staff = await _availabilityService.GetAvailableStaffAsync(
                salonId,
                serviceId,
                dateTime,
                durationMinutes,
                tenantId);

            return Ok(staff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available staff for SalonId {SalonId}, ServiceId {ServiceId}", salonId, serviceId);
            return StatusCode(500, new { error = "An error occurred while fetching available staff" });
        }
    }

    /// <summary>
    /// Get salon-wide availability for a service on a specific date
    /// </summary>
    [HttpGet("availability/salon/{salonId}")]
    [AllowAnonymous]
    public async Task<ActionResult<Dictionary<Guid, IEnumerable<AvailableTimeSlotDto>>>> GetSalonAvailability(
        Guid salonId,
        [FromQuery] Guid serviceId,
        [FromQuery] DateTime date)
    {
        try
        {
            var tenantId = GetTenantIdOrDefault();
            var availability = await _availabilityService.GetSalonAvailabilitySlotsAsync(
                salonId,
                serviceId,
                date,
                tenantId);

            return Ok(availability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salon availability for SalonId {SalonId}, ServiceId {ServiceId}", salonId, serviceId);
            return StatusCode(500, new { error = "An error occurred while fetching salon availability" });
        }
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

    private Guid GetTenantId()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Tenant ID not found in token");
        }
        return tenantId;
    }

    private Guid GetTenantIdOrDefault()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Guid.Empty; // For anonymous/public queries
        }
        return tenantId;
    }
}

public class RescheduleAppointmentDto
{
    public DateTime NewStartTime { get; set; }
    public Guid? NewStaffId { get; set; }
}

public class CancelAppointmentDto
{
    public string? CancellationReason { get; set; }
}
