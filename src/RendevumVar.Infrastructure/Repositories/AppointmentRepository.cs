using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(
        Guid customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.Salon)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Where(a => a.CustomerId == customerId);

        if (startDate.HasValue)
            query = query.Where(a => a.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.StartTime <= endDate.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStaffAsync(
        Guid staffId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Salon)
            .Where(a => a.StaffId == staffId);

        if (startDate.HasValue)
            query = query.Where(a => a.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.StartTime <= endDate.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsBySalonAsync(
        Guid salonId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Where(a => a.SalonId == salonId);

        if (startDate.HasValue)
            query = query.Where(a => a.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.StartTime <= endDate.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetAppointmentWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.Salon)
            .Include(a => a.Customer)
            .Include(a => a.Staff)
                .ThenInclude(s => s.Role)
            .Include(a => a.Service)
                .ThenInclude(s => s.Category)
            .Include(a => a.Payments)
            .Include(a => a.Review)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> HasConflictingAppointmentAsync(
        Guid staffId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Where(a => a.StaffId == staffId &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow &&
                        ((a.StartTime >= startTime && a.StartTime < endTime) ||
                         (a.EndTime > startTime && a.EndTime <= endTime) ||
                         (a.StartTime <= startTime && a.EndTime >= endTime)));

        if (excludeAppointmentId.HasValue)
            query = query.Where(a => a.Id != excludeAppointmentId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsInRangeAsync(
        Guid tenantId,
        DateTime startDate,
        DateTime endDate,
        Guid? salonId = null,
        Guid? staffId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Include(a => a.Salon)
            .Where(a => a.TenantId == tenantId &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);

        if (salonId.HasValue)
            query = query.Where(a => a.SalonId == salonId.Value);

        if (staffId.HasValue)
            query = query.Where(a => a.StaffId == staffId.Value);

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(
        Guid tenantId,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(days);

        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Include(a => a.Salon)
            .Where(a => a.TenantId == tenantId &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate &&
                        (a.Status == AppointmentStatus.Pending ||
                         a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsForRemindersAsync(
        DateTime reminderTime,
        CancellationToken cancellationToken = default)
    {
        // Get appointments that start within 24-25 hours and haven't been reminded yet
        var startWindow = reminderTime.AddHours(24);
        var endWindow = reminderTime.AddHours(25);

        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Service)
            .Include(a => a.Salon)
            .Where(a => !a.ReminderSent &&
                        a.StartTime >= startWindow &&
                        a.StartTime < endWindow &&
                        (a.Status == AppointmentStatus.Pending ||
                         a.Status == AppointmentStatus.Confirmed))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCustomerAppointmentCountAsync(
        Guid customerId,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Where(a => a.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<DateTime>> GetAvailableTimeSlotsAsync(
        Guid staffId,
        DateTime date,
        int serviceDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        // Get staff's working hours for the given date (simplified - assumes 9 AM to 6 PM)
        var startTime = date.Date.AddHours(9);
        var endTime = date.Date.AddHours(18);

        // Get existing appointments for the staff on this date
        var existingAppointments = await _context.Appointments
            .Where(a => a.StaffId == staffId &&
                        a.StartTime >= date.Date &&
                        a.StartTime < date.Date.AddDays(1) &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.StartTime)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync(cancellationToken);

        var availableSlots = new List<DateTime>();
        var currentTime = startTime;

        while (currentTime.AddMinutes(serviceDurationMinutes) <= endTime)
        {
            var slotEnd = currentTime.AddMinutes(serviceDurationMinutes);

            // Check if this slot conflicts with any existing appointment
            var hasConflict = existingAppointments.Any(apt =>
                (currentTime >= apt.StartTime && currentTime < apt.EndTime) ||
                (slotEnd > apt.StartTime && slotEnd <= apt.EndTime) ||
                (currentTime <= apt.StartTime && slotEnd >= apt.EndTime));

            if (!hasConflict)
            {
                availableSlots.Add(currentTime);
            }

            currentTime = currentTime.AddMinutes(30); // 30-minute intervals
        }

        return availableSlots;
    }
}
