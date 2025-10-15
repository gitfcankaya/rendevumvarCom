using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Interfaces;

public interface IAvailabilityService
{
    /// <summary>
    /// Gets available staff for a specific service at a specific time
    /// </summary>
    Task<IEnumerable<StaffDto>> GetAvailableStaffAsync(
        Guid salonId,
        Guid serviceId,
        DateTime dateTime,
        int durationMinutes,
        Guid tenantId);

    /// <summary>
    /// Gets the working hours for a staff member on a specific date
    /// </summary>
    Task<WorkingHoursDto?> GetStaffWorkingHoursAsync(Guid staffId, DateTime date);

    /// <summary>
    /// Calculates optimal time slots for a staff member on a specific date
    /// considering their schedule, breaks, and existing appointments
    /// </summary>
    Task<IEnumerable<AvailableTimeSlotDto>> CalculateOptimalSlotsAsync(
        Guid staffId,
        DateTime date,
        int durationMinutes);

    /// <summary>
    /// Checks if a staff member is available during a specific time range
    /// </summary>
    Task<bool> IsStaffAvailableAsync(
        Guid staffId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null);

    /// <summary>
    /// Gets all available time slots for a salon across all staff members
    /// for a specific service on a specific date
    /// </summary>
    Task<Dictionary<Guid, IEnumerable<AvailableTimeSlotDto>>> GetSalonAvailabilitySlotsAsync(
        Guid salonId,
        Guid serviceId,
        DateTime date,
        Guid tenantId);
}
