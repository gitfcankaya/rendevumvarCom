using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Service?> GetServiceWithStaffAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .Include(s => s.Staff.Where(st => st.Status == Core.Enums.StaffStatus.Active))
                .ThenInclude(st => st.Role)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Service>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Service>> GetBySalonIdAsync(Guid salonId, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .Where(s => s.SalonId == salonId);

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Category != null ? s.Category.DisplayOrder : int.MaxValue)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Service>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Service>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .Where(s => s.Staff.Any(st => st.Id == staffId))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Service>> SearchServicesAsync(
        string? searchTerm = null,
        Guid? salonId = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? maxDuration = null,
        bool? isActive = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .Include(s => s.Tenant)
            .Include(s => s.Salon)
            .Include(s => s.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term)));
        }

        if (salonId.HasValue)
        {
            query = query.Where(s => s.SalonId == salonId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(s => s.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(s => s.Price <= maxPrice.Value);
        }

        if (maxDuration.HasValue)
        {
            query = query.Where(s => s.DurationMinutes <= maxDuration.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Category != null ? s.Category.DisplayOrder : int.MaxValue)
            .ThenBy(s => s.Price)
            .ThenBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        Guid? salonId = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? maxDuration = null,
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.Description != null && s.Description.ToLower().Contains(term)));
        }

        if (salonId.HasValue)
        {
            query = query.Where(s => s.SalonId == salonId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(s => s.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(s => s.Price <= maxPrice.Value);
        }

        if (maxDuration.HasValue)
        {
            query = query.Where(s => s.DurationMinutes <= maxDuration.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Service> CreateAsync(Service service, CancellationToken cancellationToken = default)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);
        return service;
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        _context.Services.Update(service);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FindAsync(new object[] { id }, cancellationToken);
        if (service != null)
        {
            service.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Services.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AssignStaffToServiceAsync(Guid serviceId, Guid staffId, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .Include(s => s.Staff)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);

        if (service == null)
        {
            throw new InvalidOperationException($"Service with ID {serviceId} not found.");
        }

        var staff = await _context.Staff.FindAsync(new object[] { staffId }, cancellationToken);
        if (staff == null)
        {
            throw new InvalidOperationException($"Staff with ID {staffId} not found.");
        }

        if (!service.Staff.Any(s => s.Id == staffId))
        {
            service.Staff.Add(staff);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveStaffFromServiceAsync(Guid serviceId, Guid staffId, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .Include(s => s.Staff)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);

        if (service == null)
        {
            throw new InvalidOperationException($"Service with ID {serviceId} not found.");
        }

        var staff = service.Staff.FirstOrDefault(s => s.Id == staffId);
        if (staff != null)
        {
            service.Staff.Remove(staff);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Staff>> GetServiceStaffAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services
            .Include(s => s.Staff.Where(st => st.Status == Core.Enums.StaffStatus.Active))
                .ThenInclude(st => st.Role)
            .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);

        return service?.Staff ?? Enumerable.Empty<Staff>();
    }
}
