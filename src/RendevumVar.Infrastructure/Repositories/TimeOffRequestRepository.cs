using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class TimeOffRequestRepository : Repository<TimeOffRequest>, ITimeOffRequestRepository
{
    public TimeOffRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TimeOffRequest>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .Include(t => t.Staff)
            .Where(t => t.StaffId == staffId && !t.IsDeleted)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeOffRequest>> GetPendingRequestsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .Include(t => t.Staff)
            .Where(t => 
                t.TenantId == tenantId && 
                t.Status == TimeOffStatus.Pending && 
                !t.IsDeleted)
            .OrderBy(t => t.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeOffRequest>> GetByStatusAsync(Guid tenantId, TimeOffStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .Include(t => t.Staff)
            .Where(t => 
                t.TenantId == tenantId && 
                t.Status == status && 
                !t.IsDeleted)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeOffRequest>> GetByDateRangeAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .Include(t => t.Staff)
            .Where(t => 
                t.StaffId == staffId && 
                t.StartDate <= endDate &&
                t.EndDate >= startDate &&
                !t.IsDeleted)
            .OrderBy(t => t.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeOffRequest>> GetConflictingRequestsAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .Where(t => 
                t.StaffId == staffId && 
                t.Status == TimeOffStatus.Approved &&
                t.StartDate <= endDate &&
                t.EndDate >= startDate &&
                !t.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasApprovedTimeOffAsync(Guid staffId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.TimeOffRequests
            .AnyAsync(t => 
                t.StaffId == staffId && 
                t.Status == TimeOffStatus.Approved &&
                t.StartDate <= endDate &&
                t.EndDate >= startDate &&
                !t.IsDeleted, 
                cancellationToken);
    }
}
