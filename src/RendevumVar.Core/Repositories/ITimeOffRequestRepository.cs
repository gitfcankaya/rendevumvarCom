using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface ITimeOffRequestRepository : IRepository<TimeOffRequest>
{
    // Specialized queries
    Task<IEnumerable<TimeOffRequest>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequest>> GetPendingRequestsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequest>> GetByStatusAsync(Guid tenantId, TimeOffStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequest>> GetByDateRangeAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeOffRequest>> GetConflictingRequestsAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> HasApprovedTimeOffAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
