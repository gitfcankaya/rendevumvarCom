using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Interfaces;

public interface IServiceCategoryService
{
    Task<IEnumerable<ServiceCategoryListDto>> GetAllCategoriesAsync(Guid tenantId);
    Task<IEnumerable<ServiceCategoryListDto>> GetActiveCategoriesAsync(Guid tenantId);
    Task<ServiceCategoryDto?> GetCategoryByIdAsync(Guid id, Guid tenantId);
    Task<ServiceCategoryDto> CreateCategoryAsync(CreateServiceCategoryDto dto, Guid tenantId, string userId);
    Task<ServiceCategoryDto?> UpdateCategoryAsync(Guid id, UpdateServiceCategoryDto dto, Guid tenantId, string userId);
    Task<bool> DeleteCategoryAsync(Guid id, Guid tenantId, string userId);
    Task<bool> RestoreCategoryAsync(Guid id, Guid tenantId, string userId);
    Task<bool> ReorderCategoriesAsync(Dictionary<Guid, int> orderMap, Guid tenantId, string userId);
}