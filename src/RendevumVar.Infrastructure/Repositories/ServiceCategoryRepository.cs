using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ServiceCategory?> GetCategoryWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .Include(c => c.Tenant)
            .Include(c => c.Services.Where(s => s.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ServiceCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .Include(c => c.Tenant)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceCategory>> GetByTenantIdAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .Include(c => c.Tenant)
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceCategory>> GetActiveAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceCategories
            .Include(c => c.Tenant)
            .Where(c => c.IsActive);

        if (tenantId.HasValue)
        {
            query = query.Where(c => c.TenantId == tenantId.Value || c.TenantId == null);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ServiceCategory>> GetGlobalCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories
            .Where(c => c.TenantId == null && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceCategory> CreateAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default)
    {
        _context.ServiceCategories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.ServiceCategories.FindAsync(new object[] { id }, cancellationToken);
        if (category != null)
        {
            category.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ServiceCategories.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ServiceCategories
            .Where(c => c.Name.ToLower() == name.ToLower() && c.TenantId == tenantId);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
