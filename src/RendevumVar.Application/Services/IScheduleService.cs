using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Services;

public interface IScheduleService
{
    // Schedule management
    Task<StaffScheduleDto> SetStaffScheduleAsync(Guid staffId, SetStaffScheduleDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffScheduleDto>> GetStaffScheduleAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<StaffScheduleDto> UpdateStaffScheduleAsync(Guid scheduleId, SetStaffScheduleDto dto, CancellationToken cancellationToken = default);
    Task DeleteStaffScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    
    // Availability checking
    Task<bool> CheckStaffAvailabilityAsync(Guid staffId, DateTime dateTime, int durationMinutes, CancellationToken cancellationToken = default);
    Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(Guid staffId, DateTime date, int slotDurationMinutes = 30, CancellationToken cancellationToken = default);
}
