using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Repositories;
using System.Text.Json;

namespace RendevumVar.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IStaffRepository _staffRepository;
    private readonly IStaffScheduleRepository _staffScheduleRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ILogger<AvailabilityService> _logger;

    // Default working hours if no schedule is defined
    private readonly TimeSpan _defaultStartTime = new(9, 0, 0);
    private readonly TimeSpan _defaultEndTime = new(18, 0, 0);
    private readonly int _slotIntervalMinutes = 30;

    public AvailabilityService(
        IStaffRepository staffRepository,
        IStaffScheduleRepository staffScheduleRepository,
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        ILogger<AvailabilityService> logger)
    {
        _staffRepository = staffRepository;
        _staffScheduleRepository = staffScheduleRepository;
        _appointmentRepository = appointmentRepository;
        _serviceRepository = serviceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<StaffDto>> GetAvailableStaffAsync(
        Guid salonId,
        Guid serviceId,
        DateTime dateTime,
        int durationMinutes,
        Guid tenantId)
    {
        // Get all staff for this salon who can perform this service
        var allStaff = await _staffRepository.GetStaffBySalonAsync(salonId);
        var availableStaff = new List<StaffDto>();

        var endTime = dateTime.AddMinutes(durationMinutes);

        foreach (var staff in allStaff.Where(s => s.TenantId == tenantId))
        {
            // Check if staff is available during this time
            var isAvailable = await IsStaffAvailableAsync(staff.Id, dateTime, endTime);

            if (isAvailable)
            {
                availableStaff.Add(new StaffDto
                {
                    Id = staff.Id,
                    TenantId = staff.TenantId,
                    SalonId = staff.SalonId,
                    FirstName = staff.FirstName,
                    LastName = staff.LastName,
                    FullName = $"{staff.FirstName} {staff.LastName}",
                    Email = staff.Email,
                    Phone = staff.Phone,
                    Bio = staff.Bio,
                    PhotoUrl = staff.PhotoUrl,
                    ProfilePictureUrl = staff.PhotoUrl,
                    AverageRating = staff.AverageRating,
                    Status = staff.Status,
                    InvitationStatus = staff.InvitationStatus,
                    RoleId = staff.RoleId,
                    RoleName = staff.Role?.Name
                });
            }
        }

        return availableStaff;
    }

    public async Task<WorkingHoursDto?> GetStaffWorkingHoursAsync(Guid staffId, DateTime date)
    {
        var dayOfWeek = ConvertToWorkDayOfWeek(date.DayOfWeek);
        var schedules = await _staffScheduleRepository.GetByStaffIdAsync(staffId);

        var schedule = schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsRecurring);

        if (schedule == null)
        {
            return new WorkingHoursDto
            {
                Date = date,
                IsWorkingDay = false
            };
        }

        var workingHours = new WorkingHoursDto
        {
            Date = date,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            IsWorkingDay = true
        };

        // Add break time if defined
        if (schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue)
        {
            workingHours.Breaks.Add(new BreakTimeDto
            {
                StartTime = schedule.BreakStartTime.Value,
                EndTime = schedule.BreakEndTime.Value,
                Reason = "Break"
            });
        }

        return workingHours;
    }

    private Core.Enums.WorkDayOfWeek ConvertToWorkDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => Core.Enums.WorkDayOfWeek.Sunday,
            DayOfWeek.Monday => Core.Enums.WorkDayOfWeek.Monday,
            DayOfWeek.Tuesday => Core.Enums.WorkDayOfWeek.Tuesday,
            DayOfWeek.Wednesday => Core.Enums.WorkDayOfWeek.Wednesday,
            DayOfWeek.Thursday => Core.Enums.WorkDayOfWeek.Thursday,
            DayOfWeek.Friday => Core.Enums.WorkDayOfWeek.Friday,
            DayOfWeek.Saturday => Core.Enums.WorkDayOfWeek.Saturday,
            _ => Core.Enums.WorkDayOfWeek.Monday
        };
    }

    public async Task<IEnumerable<AvailableTimeSlotDto>> CalculateOptimalSlotsAsync(
        Guid staffId,
        DateTime date,
        int durationMinutes)
    {
        var availableSlots = new List<AvailableTimeSlotDto>();

        // Get staff working hours for this date
        var workingHours = await GetStaffWorkingHoursAsync(staffId, date);

        if (workingHours == null || !workingHours.IsWorkingDay)
        {
            return availableSlots;
        }

        // Get existing appointments for this staff on this date
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);
        var existingAppointments = await _appointmentRepository.GetAppointmentsByStaffAsync(
            staffId,
            startOfDay,
            endOfDay);

        // Generate time slots based on working hours
        var currentSlotTime = workingHours.StartTime;
        var endTime = workingHours.EndTime;

        while (currentSlotTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= endTime)
        {
            var slotStart = date.Date.Add(currentSlotTime);
            var slotEnd = slotStart.AddMinutes(durationMinutes);

            // Check if slot is in the past
            if (slotStart <= DateTime.UtcNow)
            {
                currentSlotTime = currentSlotTime.Add(TimeSpan.FromMinutes(_slotIntervalMinutes));
                continue;
            }

            // Check if slot conflicts with a break
            var hasBreakConflict = workingHours.Breaks.Any(b =>
                (currentSlotTime >= b.StartTime && currentSlotTime < b.EndTime) ||
                (currentSlotTime.Add(TimeSpan.FromMinutes(durationMinutes)) > b.StartTime &&
                 currentSlotTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= b.EndTime));

            if (hasBreakConflict)
            {
                currentSlotTime = currentSlotTime.Add(TimeSpan.FromMinutes(_slotIntervalMinutes));
                continue;
            }

            // Check if slot conflicts with existing appointments
            var hasAppointmentConflict = existingAppointments.Any(a =>
                (slotStart >= a.StartTime && slotStart < a.EndTime) ||
                (slotEnd > a.StartTime && slotEnd <= a.EndTime) ||
                (slotStart <= a.StartTime && slotEnd >= a.EndTime));

            if (!hasAppointmentConflict)
            {
                availableSlots.Add(new AvailableTimeSlotDto
                {
                    Date = date,
                    StartTime = currentSlotTime,
                    EndTime = currentSlotTime.Add(TimeSpan.FromMinutes(durationMinutes)),
                    DurationMinutes = durationMinutes
                });
            }

            currentSlotTime = currentSlotTime.Add(TimeSpan.FromMinutes(_slotIntervalMinutes));
        }

        return availableSlots;
    }

    public async Task<bool> IsStaffAvailableAsync(
        Guid staffId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAppointmentId = null)
    {
        // Check working hours
        var workingHours = await GetStaffWorkingHoursAsync(staffId, startTime.Date);

        if (workingHours == null || !workingHours.IsWorkingDay)
        {
            return false;
        }

        var requestedStartTime = startTime.TimeOfDay;
        var requestedEndTime = endTime.TimeOfDay;

        // Check if within working hours
        if (requestedStartTime < workingHours.StartTime || requestedEndTime > workingHours.EndTime)
        {
            return false;
        }

        // Check if conflicts with breaks
        var hasBreakConflict = workingHours.Breaks.Any(b =>
            (requestedStartTime >= b.StartTime && requestedStartTime < b.EndTime) ||
            (requestedEndTime > b.StartTime && requestedEndTime <= b.EndTime) ||
            (requestedStartTime <= b.StartTime && requestedEndTime >= b.EndTime));

        if (hasBreakConflict)
        {
            return false;
        }

        // Check for appointment conflicts
        var hasAppointmentConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            staffId,
            startTime,
            endTime,
            excludeAppointmentId);

        return !hasAppointmentConflict;
    }

    public async Task<Dictionary<Guid, IEnumerable<AvailableTimeSlotDto>>> GetSalonAvailabilitySlotsAsync(
        Guid salonId,
        Guid serviceId,
        DateTime date,
        Guid tenantId)
    {
        var result = new Dictionary<Guid, IEnumerable<AvailableTimeSlotDto>>();

        // Get service to know the duration
        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null || service.TenantId != tenantId)
        {
            return result;
        }

        // Get all staff for this salon
        var allStaff = await _staffRepository.GetStaffBySalonAsync(salonId);
        var salonStaff = allStaff.Where(s => s.TenantId == tenantId && s.Status == Core.Enums.StaffStatus.Active);

        foreach (var staff in salonStaff)
        {
            var slots = await CalculateOptimalSlotsAsync(staff.Id, date, service.DurationMinutes);
            if (slots.Any())
            {
                result[staff.Id] = slots;
            }
        }

        return result;
    }
}
