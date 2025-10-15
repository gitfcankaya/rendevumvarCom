using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface IStaffScheduleRepository : IRepository<StaffSchedule>
{
    // Specialized queries
    Task<IEnumerable<StaffSchedule>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffSchedule>> GetByStaffAndDayAsync(Guid staffId, WorkDayOfWeek dayOfWeek, CancellationToken cancellationToken = default);
    Task<StaffSchedule?> GetByStaffAndDateAsync(Guid staffId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffSchedule>> GetActiveSchedulesAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffSchedule>> GetRecurringSchedulesAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StaffSchedule>> GetByDateRangeAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
