using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    // Specialized queries
    Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Staff?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Staff?> GetWithRoleAsync(Guid staffId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetActiveStaffAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetStaffByStatusAsync(Guid tenantId, StaffStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetStaffBySalonAsync(Guid salonId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetStaffByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeStaffId = null, CancellationToken cancellationToken = default);
    Task<int> CountByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
