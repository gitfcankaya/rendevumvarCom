using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class SalonRepository : ISalonRepository
{
    private readonly ApplicationDbContext _context;

    public SalonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Salon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Salon?> GetSalonWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Salon?> GetSalonWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Services.Where(svc => svc.IsActive))
                .ThenInclude(svc => svc.Category)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Salon?> GetSalonWithStaffAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Staff.Where(st => st.Status == Core.Enums.StaffStatus.Active))
                .ThenInclude(st => st.Role)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Salon?> GetSalonFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Images.OrderBy(i => i.DisplayOrder))
            .Include(s => s.Services.Where(svc => svc.IsActive))
                .ThenInclude(svc => svc.Category)
            .Include(s => s.Staff.Where(st => st.Status == Core.Enums.StaffStatus.Active))
                .ThenInclude(st => st.Role)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Salon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Salon>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Salons
            .Include(s => s.Tenant)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Salon>> SearchSalonsAsync(
        string? searchTerm = null,
        string? city = null,
        decimal? minRating = null,
        bool? isActive = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Images.Where(i => i.IsPrimary))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Description.ToLower().Contains(term) ||
                s.City.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(s => s.City.ToLower() == city.ToLower());
        }

        if (minRating.HasValue)
        {
            query = query.Where(s => s.AverageRating >= minRating.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(s => s.AverageRating)
            .ThenBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        string? city = null,
        decimal? minRating = null,
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Salons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Description.ToLower().Contains(term) ||
                s.City.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(s => s.City.ToLower() == city.ToLower());
        }

        if (minRating.HasValue)
        {
            query = query.Where(s => s.AverageRating >= minRating.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Salon>> GetSalonsByLocationAsync(
        decimal latitude,
        decimal longitude,
        double radiusKm,
        CancellationToken cancellationToken = default)
    {
        // Simple distance calculation using Haversine formula
        // For production, consider using a spatial database extension
        var salons = await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Images.Where(i => i.IsPrimary))
            .Where(s => s.IsActive && s.Latitude.HasValue && s.Longitude.HasValue)
            .ToListAsync(cancellationToken);

        return salons.Where(s =>
        {
            var distance = CalculateDistance(latitude, longitude, s.Latitude!.Value, s.Longitude!.Value);
            return distance <= radiusKm;
        }).OrderBy(s =>
            CalculateDistance(latitude, longitude, s.Latitude!.Value, s.Longitude!.Value)
        );
    }

    public async Task<IEnumerable<Salon>> GetSalonsByServiceIdsAsync(
        IEnumerable<Guid> serviceIds,
        CancellationToken cancellationToken = default)
    {
        var serviceIdList = serviceIds.ToList();
        
        return await _context.Salons
            .Include(s => s.Tenant)
            .Include(s => s.Images.Where(i => i.IsPrimary))
            .Where(s => s.IsActive && s.Services.Any(svc => serviceIdList.Contains(svc.Id) && svc.IsActive))
            .Distinct()
            .OrderByDescending(s => s.AverageRating)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Salon> CreateAsync(Salon salon, CancellationToken cancellationToken = default)
    {
        _context.Salons.Add(salon);
        await _context.SaveChangesAsync(cancellationToken);
        return salon;
    }

    public async Task UpdateAsync(Salon salon, CancellationToken cancellationToken = default)
    {
        _context.Salons.Update(salon);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var salon = await _context.Salons.FindAsync(new object[] { id }, cancellationToken);
        if (salon != null)
        {
            salon.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Salons.AnyAsync(s => s.Id == id, cancellationToken);
    }

    // Helper method to calculate distance between two coordinates (Haversine formula)
    private double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double R = 6371; // Radius of the Earth in kilometers

        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
