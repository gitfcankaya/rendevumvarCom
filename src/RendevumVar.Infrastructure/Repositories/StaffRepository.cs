using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .FirstOrDefaultAsync(s => s.Email == email && !s.IsDeleted, cancellationToken);
    }

    public async Task<Staff?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .FirstOrDefaultAsync(s => 
                s.InvitationToken == token && 
                s.InvitationStatus == InvitationStatus.Pending &&
                s.InvitationExpiresAt > DateTime.UtcNow &&
                !s.IsDeleted, 
                cancellationToken);
    }

    public async Task<Staff?> GetWithRoleAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == staffId && !s.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetActiveStaffAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .Where(s => 
                s.TenantId == tenantId && 
                s.Status == StaffStatus.Active && 
                !s.IsDeleted)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetStaffByStatusAsync(Guid tenantId, StaffStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .Where(s => 
                s.TenantId == tenantId && 
                s.Status == status && 
                !s.IsDeleted)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetStaffBySalonAsync(Guid salonId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.User)
            .Where(s => 
                s.SalonId == salonId && 
                !s.IsDeleted)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetStaffByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Role)
            .Include(s => s.Salon)
            .Where(s => 
                s.RoleId == roleId && 
                !s.IsDeleted)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeStaffId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Staff.Where(s => s.Email == email && !s.IsDeleted);
        
        if (excludeStaffId.HasValue)
        {
            query = query.Where(s => s.Id != excludeStaffId.Value);
        }
        
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Where(s => s.TenantId == tenantId && !s.IsDeleted)
            .CountAsync(cancellationToken);
    }
}
