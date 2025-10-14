using Microsoft.EntityFrameworkCore;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Application.Services;

public class ServiceService : IServiceService
{
    private readonly ApplicationDbContext _context;

    public ServiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceListDto>> GetAllServicesAsync(Guid tenantId)
    {
        return await _context.Services
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceListDto
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category != null ? s.Category.Name : null,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive && !s.IsDeleted,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceListDto>> GetActiveServicesAsync(Guid tenantId)
    {
        return await _context.Services
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceListDto
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category != null ? s.Category.Name : null,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(Guid id, Guid tenantId)
    {
        var service = await _context.Services
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (service == null) return null;

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Category = service.Category?.Name,
            IsActive = service.IsActive,
            Notes = service.Description, // Using description as notes for now
            ImageUrl = service.ImageUrl,
            CreatedAt = service.CreatedAt,
            CreatedBy = service.CreatedBy,
            UpdatedAt = service.UpdatedAt,
            UpdatedBy = service.UpdatedBy
        };
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, Guid tenantId, string userId)
    {
        var service = new Service
        {
            TenantId = tenantId,
            SalonId = Guid.NewGuid(), // This should come from context or be set properly
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            ImageUrl = dto.ImageUrl,
            IsActive = true
        };

        service.SetCreated(userId);

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Category = dto.Category,
            IsActive = service.IsActive,
            Notes = dto.Notes,
            ImageUrl = service.ImageUrl,
            CreatedAt = service.CreatedAt,
            CreatedBy = service.CreatedBy,
            UpdatedAt = service.UpdatedAt,
            UpdatedBy = service.UpdatedBy
        };
    }

    public async Task<ServiceDto?> UpdateServiceAsync(Guid id, UpdateServiceDto dto, Guid tenantId, string userId)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId && !s.IsDeleted);

        if (service == null) return null;

        service.Name = dto.Name;
        service.Description = dto.Description;
        service.Price = dto.Price;
        service.DurationMinutes = dto.DurationMinutes;
        service.ImageUrl = dto.ImageUrl;
        service.IsActive = dto.IsActive;
        
        service.SetUpdated(userId);

        await _context.SaveChangesAsync();

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Category = dto.Category,
            IsActive = service.IsActive,
            Notes = dto.Notes,
            ImageUrl = service.ImageUrl,
            CreatedAt = service.CreatedAt,
            CreatedBy = service.CreatedBy,
            UpdatedAt = service.UpdatedAt,
            UpdatedBy = service.UpdatedBy
        };
    }

    public async Task<bool> DeleteServiceAsync(Guid id, Guid tenantId, string userId)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId && !s.IsDeleted);

        if (service == null) return false;

        service.SetDeleted(userId);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreServiceAsync(Guid id, Guid tenantId, string userId)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId && s.IsDeleted);

        if (service == null) return false;

        service.Restore(userId);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ServiceListDto>> GetServicesByCategoryAsync(string category, Guid tenantId)
    {
        return await _context.Services
            .Include(s => s.Category)
            .Where(s => s.TenantId == tenantId && 
                       s.Category!.Name == category && 
                       s.IsActive && 
                       !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceListDto
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category!.Name,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(Guid tenantId)
    {
        return await _context.ServiceCategories
            .Where(sc => sc.TenantId == tenantId && !sc.IsDeleted)
            .OrderBy(sc => sc.Name)
            .Select(sc => sc.Name)
            .ToListAsync();
    }
}