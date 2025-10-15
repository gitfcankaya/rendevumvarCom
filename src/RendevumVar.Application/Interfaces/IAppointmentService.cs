using RendevumVar.Application.DTOs;
using RendevumVar.Core.Enums;

namespace RendevumVar.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto, Guid customerId, Guid tenantId);
    Task<AppointmentDetailsDto?> GetAppointmentDetailsAsync(Guid id, Guid tenantId);
    Task<AppointmentDto> UpdateAppointmentAsync(UpdateAppointmentDto dto, Guid tenantId);
    Task<AppointmentDto> UpdateAppointmentStatusAsync(Guid id, UpdateAppointmentStatusDto dto, Guid tenantId);
    Task CancelAppointmentAsync(Guid id, string? cancellationReason, Guid tenantId);
    Task<AppointmentDto> RescheduleAppointmentAsync(Guid id, DateTime newStartTime, Guid? newStaffId, Guid tenantId);
    Task<IEnumerable<AppointmentDto>> GetCustomerAppointmentsAsync(Guid customerId, DateTime? startDate = null, DateTime? endDate = null, AppointmentStatus? status = null);
    Task<IEnumerable<AppointmentDto>> GetStaffAppointmentsAsync(Guid staffId, Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<AppointmentDto>> GetSalonAppointmentsAsync(Guid salonId, Guid tenantId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(Guid staffId, DateTime date, int serviceDurationMinutes);
    Task<bool> HasConflictingAppointmentAsync(Guid staffId, DateTime startTime, DateTime endTime, Guid? excludeAppointmentId = null);
}
