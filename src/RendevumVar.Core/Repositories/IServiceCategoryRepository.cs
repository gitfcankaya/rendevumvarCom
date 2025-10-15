using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface IServiceCategoryRepository
{
    Task<ServiceCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceCategory?> GetCategoryWithServicesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceCategory>> GetByTenantIdAsync(Guid? tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceCategory>> GetActiveAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceCategory>> GetGlobalCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ServiceCategory> CreateAsync(ServiceCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? tenantId, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
