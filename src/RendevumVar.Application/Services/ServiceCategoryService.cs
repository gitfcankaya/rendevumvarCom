using Microsoft.EntityFrameworkCore;
using RendevumVar.Application.DTOs;
using RendevumVar.Application.Interfaces;
using RendevumVar.Core.Entities;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Application.Services;

public class ServiceCategoryService : IServiceCategoryService
{
    private readonly ApplicationDbContext _context;

    public ServiceCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ServiceCategoryListDto>> GetAllCategoriesAsync(Guid tenantId)
    {
        return await _context.ServiceCategories
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ServiceCategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive && !c.IsDeleted,
                ServiceCount = c.Services.Count(s => !s.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceCategoryListDto>> GetActiveCategoriesAsync(Guid tenantId)
    {
        return await _context.ServiceCategories
            .Where(c => c.TenantId == tenantId && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ServiceCategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ServiceCount = c.Services.Count(s => !s.IsDeleted),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ServiceCategoryDto?> GetCategoryByIdAsync(Guid id, Guid tenantId)
    {
        var category = await _context.ServiceCategories
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (category == null) return null;

        return new ServiceCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ServiceCount = category.Services.Count(s => !s.IsDeleted),
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy
        };
    }

    public async Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto, Guid tenantId, string userId)
    {
        var category = new ServiceCategory
        {
            TenantId = tenantId,
            Name = dto.Name,
            Description = dto.Description,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true
        };

        category.SetCreated(userId);

        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync();

        return new ServiceCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ServiceCount = 0,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy
        };
    }

    public async Task<ServiceCategoryDto?> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto, Guid tenantId, string userId)
    {
        var category = await _context.ServiceCategories
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (category == null) return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;
        
        category.SetUpdated(userId);

        await _context.SaveChangesAsync();

        return new ServiceCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ServiceCount = category.Services.Count(s => !s.IsDeleted),
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy
        };
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, Guid tenantId, string userId)
    {
        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (category == null) return false;

        // Check if category has services
        var hasServices = await _context.Services
            .AnyAsync(s => s.CategoryId == id && !s.IsDeleted);

        if (hasServices)
        {
            throw new InvalidOperationException("Bu kategoriye ait hizmetler bulunmaktadır. Önce hizmetleri başka kategoriye taşıyınız.");
        }

        category.SetDeleted(userId);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreCategoryAsync(Guid id, Guid tenantId, string userId)
    {
        var category = await _context.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && c.IsDeleted);

        if (category == null) return false;

        category.Restore(userId);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderCategoriesAsync(Dictionary<Guid, int> orderMap, Guid tenantId, string userId)
    {
        foreach (var (categoryId, order) in orderMap)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.TenantId == tenantId && !c.IsDeleted);

            if (category != null)
            {
                category.DisplayOrder = order;
                category.SetUpdated(userId);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }
}