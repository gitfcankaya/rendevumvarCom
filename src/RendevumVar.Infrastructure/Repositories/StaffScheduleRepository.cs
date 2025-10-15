using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class StaffScheduleRepository : Repository<StaffSchedule>, IStaffScheduleRepository
{
    public StaffScheduleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StaffSchedule>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.StaffSchedules
            .Where(s => s.StaffId == staffId && !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StaffSchedule>> GetByStaffAndDayAsync(Guid staffId, WorkDayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await _context.StaffSchedules
            .Where(s => 
                s.StaffId == staffId && 
                s.DayOfWeek == dayOfWeek && 
                s.IsActive &&
                !s.IsDeleted)
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffSchedule?> GetByStaffAndDateAsync(Guid staffId, DateTime date, CancellationToken cancellationToken = default)
    {
        var dayOfWeek = ConvertToDayOfWeek(date.DayOfWeek);
        
        // First check for specific date schedule (exception)
        var specificSchedule = await _context.StaffSchedules
            .FirstOrDefaultAsync(s => 
                s.StaffId == staffId && 
                s.SpecificDate.HasValue &&
                s.SpecificDate.Value.Date == date.Date &&
                s.IsActive &&
                !s.IsDeleted, 
                cancellationToken);
        
        if (specificSchedule != null)
            return specificSchedule;
        
        // Then check for recurring schedule
        return await _context.StaffSchedules
            .FirstOrDefaultAsync(s => 
                s.StaffId == staffId && 
                s.DayOfWeek == dayOfWeek && 
                s.IsRecurring &&
                s.IsActive &&
                !s.IsDeleted, 
                cancellationToken);
    }

    public async Task<IEnumerable<StaffSchedule>> GetActiveSchedulesAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.StaffSchedules
            .Where(s => 
                s.StaffId == staffId && 
                s.IsActive && 
                !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StaffSchedule>> GetRecurringSchedulesAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.StaffSchedules
            .Where(s => 
                s.StaffId == staffId && 
                s.IsRecurring && 
                s.IsActive &&
                !s.IsDeleted)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StaffSchedule>> GetByDateRangeAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var schedules = new List<StaffSchedule>();
        
        // Get all recurring schedules
        var recurringSchedules = await GetRecurringSchedulesAsync(staffId, cancellationToken);
        
        // Get specific date schedules within range
        var specificSchedules = await _context.StaffSchedules
            .Where(s => 
                s.StaffId == staffId && 
                s.SpecificDate.HasValue &&
                s.SpecificDate.Value.Date >= startDate.Date &&
                s.SpecificDate.Value.Date <= endDate.Date &&
                s.IsActive &&
                !s.IsDeleted)
            .ToListAsync(cancellationToken);
        
        schedules.AddRange(recurringSchedules);
        schedules.AddRange(specificSchedules);
        
        return schedules;
    }

    private WorkDayOfWeek ConvertToDayOfWeek(DayOfWeek systemDayOfWeek)
    {
        return systemDayOfWeek switch
        {
            DayOfWeek.Monday => WorkDayOfWeek.Monday,
            DayOfWeek.Tuesday => WorkDayOfWeek.Tuesday,
            DayOfWeek.Wednesday => WorkDayOfWeek.Wednesday,
            DayOfWeek.Thursday => WorkDayOfWeek.Thursday,
            DayOfWeek.Friday => WorkDayOfWeek.Friday,
            DayOfWeek.Saturday => WorkDayOfWeek.Saturday,
            DayOfWeek.Sunday => WorkDayOfWeek.Sunday,
            _ => WorkDayOfWeek.Monday
        };
    }
}
