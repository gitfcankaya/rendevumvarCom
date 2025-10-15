using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    /// <summary>
    /// Get appointments by customer ID with optional filters
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(
        Guid customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get appointments by staff ID with optional filters
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsByStaffAsync(
        Guid staffId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get appointments by salon ID with optional filters
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsBySalonAsync(
        Guid salonId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get appointment with all related entities
    /// </summary>
    Task<Appointment?> GetAppointmentWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if staff has conflicting appointments
    /// </summary>
    Task<bool> HasConflictingAppointmentAsync(
        Guid staffId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get appointments for a date range
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsInRangeAsync(
        Guid tenantId,
        DateTime startDate,
        DateTime endDate,
        Guid? salonId = null,
        Guid? staffId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming appointments (pending or confirmed)
    /// </summary>
    Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(
        Guid tenantId,
        int days = 7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get appointments that need reminders (24 hours before)
    /// </summary>
    Task<IEnumerable<Appointment>> GetAppointmentsForRemindersAsync(
        DateTime reminderTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer's appointment history count
    /// </summary>
    Task<int> GetCustomerAppointmentCountAsync(
        Guid customerId,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available time slots for a staff member on a specific date
    /// </summary>
    Task<IEnumerable<DateTime>> GetAvailableTimeSlotsAsync(
        Guid staffId,
        DateTime date,
        int serviceDurationMinutes,
        CancellationToken cancellationToken = default);
}
