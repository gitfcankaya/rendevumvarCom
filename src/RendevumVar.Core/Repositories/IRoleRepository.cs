using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    // Specialized queries
    Task<Role?> GetByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsNameUniqueAsync(Guid tenantId, string name, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
}
