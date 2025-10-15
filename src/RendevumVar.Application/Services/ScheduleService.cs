using Microsoft.Extensions.Logging;
using RendevumVar.Application.DTOs;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;

namespace RendevumVar.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IStaffScheduleRepository _scheduleRepository;
    private readonly ITimeOffRequestRepository _timeOffRepository;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IStaffScheduleRepository scheduleRepository,
        ITimeOffRequestRepository timeOffRepository,
        ILogger<ScheduleService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _timeOffRepository = timeOffRepository;
        _logger = logger;
    }

    public async Task<StaffScheduleDto> SetStaffScheduleAsync(Guid staffId, SetStaffScheduleDto dto, CancellationToken cancellationToken = default)
    {
        // Validate times
        if (dto.StartTime >= dto.EndTime)
        {
            throw new InvalidOperationException("Bitiş saati başlangıç saatinden sonra olmalıdır.");
        }

        if (dto.BreakStartTime.HasValue && dto.BreakEndTime.HasValue)
        {
            if (dto.BreakStartTime >= dto.BreakEndTime)
            {
                throw new InvalidOperationException("Mola bitiş saati başlangıç saatinden sonra olmalıdır.");
            }

            if (dto.BreakStartTime < dto.StartTime || dto.BreakEndTime > dto.EndTime)
            {
                throw new InvalidOperationException("Mola çalışma saatleri içinde olmalıdır.");
            }
        }

        // Check for conflicts
        if (dto.IsRecurring && dto.DayOfWeek.HasValue)
        {
            var existingSchedules = await _scheduleRepository.GetByStaffAndDayAsync(staffId, dto.DayOfWeek.Value);
            if (existingSchedules.Any(s => s.IsActive && s.IsRecurring))
            {
                throw new InvalidOperationException("Bu gün için zaten aktif bir çalışma programı var.");
            }
        }
        else if (!dto.IsRecurring && dto.SpecificDate.HasValue)
        {
            var existingSchedule = await _scheduleRepository.GetByStaffAndDateAsync(staffId, dto.SpecificDate.Value);
            if (existingSchedule != null && existingSchedule.IsActive)
            {
                throw new InvalidOperationException("Bu tarih için zaten aktif bir çalışma programı var.");
            }
        }

        var schedule = new StaffSchedule
        {
            Id = Guid.NewGuid(),
            StaffId = staffId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakStartTime = dto.BreakStartTime,
            BreakEndTime = dto.BreakEndTime,
            IsRecurring = dto.IsRecurring,
            SpecificDate = dto.SpecificDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await _scheduleRepository.AddAsync(schedule);

        _logger.LogInformation("Schedule created for staff {StaffId}", staffId);

        return MapToScheduleDto(schedule);
    }

    public async Task<IEnumerable<StaffScheduleDto>> GetStaffScheduleAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleRepository.GetByStaffIdAsync(staffId);
        return schedules.Select(MapToScheduleDto);
    }

    public async Task<StaffScheduleDto> UpdateStaffScheduleAsync(Guid scheduleId, SetStaffScheduleDto dto, CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Çalışma programı bulunamadı.");
        }

        // Validate times
        if (dto.StartTime >= dto.EndTime)
        {
            throw new InvalidOperationException("Bitiş saati başlangıç saatinden sonra olmalıdır.");
        }

        if (dto.BreakStartTime.HasValue && dto.BreakEndTime.HasValue)
        {
            if (dto.BreakStartTime >= dto.BreakEndTime)
            {
                throw new InvalidOperationException("Mola bitiş saati başlangıç saatinden sonra olmalıdır.");
            }

            if (dto.BreakStartTime < dto.StartTime || dto.BreakEndTime > dto.EndTime)
            {
                throw new InvalidOperationException("Mola çalışma saatleri içinde olmalıdır.");
            }
        }

        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.BreakStartTime = dto.BreakStartTime;
        schedule.BreakEndTime = dto.BreakEndTime;
        schedule.IsRecurring = dto.IsRecurring;
        schedule.SpecificDate = dto.SpecificDate;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);

        _logger.LogInformation("Schedule updated: {ScheduleId}", scheduleId);

        return MapToScheduleDto(schedule);
    }

    public async Task DeleteStaffScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Çalışma programı bulunamadı.");
        }

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);

        _logger.LogInformation("Schedule deleted: {ScheduleId}", scheduleId);
    }

    public async Task<bool> CheckStaffAvailabilityAsync(
        Guid staffId,
        DateTime dateTime,
        int durationMinutes,
        CancellationToken cancellationToken = default)
    {
        var endTime = dateTime.AddMinutes(durationMinutes);

        // Check if staff has approved time off
        if (await _timeOffRepository.HasApprovedTimeOffAsync(staffId, dateTime, endTime, cancellationToken))
        {
            return false;
        }

        // Get schedule for the date
        var schedule = await _scheduleRepository.GetByStaffAndDateAsync(staffId, dateTime.Date);
        if (schedule == null || !schedule.IsActive)
        {
            return false;
        }

        // Check if time is within working hours
        var appointmentTime = dateTime.TimeOfDay;
        var appointmentEndTime = endTime.TimeOfDay;

        if (appointmentTime < schedule.StartTime || appointmentEndTime > schedule.EndTime)
        {
            return false;
        }

        // Check if time overlaps with break
        if (schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue)
        {
            if (!(appointmentEndTime <= schedule.BreakStartTime || appointmentTime >= schedule.BreakEndTime))
            {
                return false; // Overlaps with break
            }
        }

        return true;
    }

    public async Task<IEnumerable<AvailableTimeSlotDto>> GetAvailableTimeSlotsAsync(
        Guid staffId,
        DateTime date,
        int slotDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        var availableSlots = new List<AvailableTimeSlotDto>();

        // Get schedule for the date
        var schedule = await _scheduleRepository.GetByStaffAndDateAsync(staffId, date.Date);
        if (schedule == null || !schedule.IsActive)
        {
            return availableSlots;
        }

        // Check if staff has time off on this date
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);
        if (await _timeOffRepository.HasApprovedTimeOffAsync(staffId, startOfDay, endOfDay, cancellationToken))
        {
            return availableSlots;
        }

        // Generate time slots
        var currentTime = schedule.StartTime;
        var endTime = schedule.EndTime;

        while (currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes)) <= endTime)
        {
            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(slotDurationMinutes));

            // Check if slot overlaps with break
            var isBreakOverlap = false;
            if (schedule.BreakStartTime.HasValue && schedule.BreakEndTime.HasValue)
            {
                if (!(slotEndTime <= schedule.BreakStartTime || currentTime >= schedule.BreakEndTime))
                {
                    isBreakOverlap = true;
                }
            }

            if (!isBreakOverlap)
            {
                availableSlots.Add(new AvailableTimeSlotDto
                {
                    Date = date.Date,
                    StartTime = currentTime,
                    EndTime = slotEndTime,
                    DurationMinutes = slotDurationMinutes
                });
            }

            currentTime = slotEndTime;
        }

        return availableSlots;
    }

    // Helper method
    private StaffScheduleDto MapToScheduleDto(StaffSchedule schedule)
    {
        return new StaffScheduleDto
        {
            Id = schedule.Id,
            StaffId = schedule.StaffId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            BreakStartTime = schedule.BreakStartTime,
            BreakEndTime = schedule.BreakEndTime,
            IsRecurring = schedule.IsRecurring,
            SpecificDate = schedule.SpecificDate,
            IsActive = schedule.IsActive
        };
    }
}
