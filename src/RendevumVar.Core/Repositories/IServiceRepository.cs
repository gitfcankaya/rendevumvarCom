using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Service?> GetServiceWithStaffAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetBySalonIdAsync(Guid salonId, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Service>> SearchServicesAsync(
        string? searchTerm = null,
        Guid? salonId = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? maxDuration = null,
        bool? isActive = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        Guid? salonId = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? maxDuration = null,
        bool? isActive = true,
        CancellationToken cancellationToken = default);
    Task<Service> CreateAsync(Service service, CancellationToken cancellationToken = default);
    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AssignStaffToServiceAsync(Guid serviceId, Guid staffId, CancellationToken cancellationToken = default);
    Task RemoveStaffFromServiceAsync(Guid serviceId, Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetServiceStaffAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
