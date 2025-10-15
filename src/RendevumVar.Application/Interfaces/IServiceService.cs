using RendevumVar.Application.DTOs;

namespace RendevumVar.Application.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceListDto>> GetAllServicesAsync(Guid tenantId);
    Task<IEnumerable<ServiceListDto>> GetActiveServicesAsync(Guid tenantId);
    Task<ServiceDto?> GetServiceByIdAsync(Guid id, Guid tenantId);
    Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, Guid tenantId, string userId);
    Task<ServiceDto?> UpdateServiceAsync(Guid id, UpdateServiceDto dto, Guid tenantId, string userId);
    Task<bool> DeleteServiceAsync(Guid id, Guid tenantId, string userId);
    Task<bool> RestoreServiceAsync(Guid id, Guid tenantId, string userId);
    Task<IEnumerable<ServiceListDto>> GetServicesByCategoryAsync(string category, Guid tenantId);
    Task<IEnumerable<string>> GetCategoriesAsync(Guid tenantId);
    
    // Phase 3: Salon-specific service methods
    Task<IEnumerable<ServiceDto>> GetServicesBySalonIdAsync(Guid salonId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<ServiceDto?> GetServiceWithStaffAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task AssignStaffToServiceAsync(Guid serviceId, Guid staffId, Guid tenantId, CancellationToken cancellationToken = default);
    Task RemoveStaffFromServiceAsync(Guid serviceId, Guid staffId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<StaffDto>> GetServiceStaffAsync(Guid serviceId, Guid tenantId, CancellationToken cancellationToken = default);
}