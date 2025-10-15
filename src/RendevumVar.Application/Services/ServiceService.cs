using Microsoft.EntityFrameworkCore;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Application.Services;

public class ServiceService : IServiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceRepository _serviceRepository;

    public ServiceService(ApplicationDbContext context, IServiceRepository serviceRepository)
    {
        _context = context;
        _serviceRepository = serviceRepository;
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

    // Phase 3: Salon-specific service methods
    public async Task<IEnumerable<ServiceDto>> GetServicesBySalonIdAsync(Guid salonId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var services = await _serviceRepository.GetBySalonIdAsync(salonId, true, cancellationToken);

        return services.Where(s => s.TenantId == tenantId).Select(s => new ServiceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            ImageUrl = s.ImageUrl,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy
        });
    }

    public async Task<ServiceDto?> GetServiceWithStaffAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetServiceWithStaffAsync(id, cancellationToken);

        if (service == null || service.TenantId != tenantId)
            return null;

        return new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            ImageUrl = service.ImageUrl,
            CategoryId = service.CategoryId,
            CategoryName = service.Category?.Name,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt,
            CreatedBy = service.CreatedBy,
            UpdatedAt = service.UpdatedAt,
            UpdatedBy = service.UpdatedBy,
            StaffIds = service.Staff?.Select(st => st.Id).ToList() ?? new List<Guid>()
        };
    }

    public async Task AssignStaffToServiceAsync(Guid serviceId, Guid staffId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service == null || service.TenantId != tenantId)
            throw new InvalidOperationException($"Service with ID {serviceId} not found or access denied.");

        await _serviceRepository.AssignStaffToServiceAsync(serviceId, staffId, cancellationToken);
    }

    public async Task RemoveStaffFromServiceAsync(Guid serviceId, Guid staffId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service == null || service.TenantId != tenantId)
            throw new InvalidOperationException($"Service with ID {serviceId} not found or access denied.");

        await _serviceRepository.RemoveStaffFromServiceAsync(serviceId, staffId, cancellationToken);
    }

    public async Task<List<StaffDto>> GetServiceStaffAsync(Guid serviceId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);

        if (service == null || service.TenantId != tenantId)
            return new List<StaffDto>();

        var staff = await _serviceRepository.GetServiceStaffAsync(serviceId, cancellationToken);

        return staff.Select(st => new StaffDto
        {
            Id = st.Id,
            FirstName = st.FirstName,
            LastName = st.LastName,
            FullName = $"{st.FirstName} {st.LastName}",
            Email = st.Email,
            Phone = st.Phone,
            Bio = st.Bio,
            AverageRating = st.AverageRating,
            ProfilePictureUrl = st.PhotoUrl,
            RoleId = st.RoleId,
            RoleName = st.Role?.Name,
            Status = st.Status
        }).ToList();
    }
}