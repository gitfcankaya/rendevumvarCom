using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => 
                r.TenantId == tenantId && 
                r.Name == name && 
                !r.IsDeleted, 
                cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.TenantId == tenantId && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsSystemRole && !r.IsDeleted)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(Guid tenantId, string name, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r => 
            r.TenantId == tenantId && 
            r.Name == name && 
            !r.IsDeleted);
        
        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }
        
        return !await query.AnyAsync(cancellationToken);
    }
}
