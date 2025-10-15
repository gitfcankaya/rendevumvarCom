using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface ITenantSubscriptionRepository : IRepository<TenantSubscription>
{
    Task<TenantSubscription?> GetCurrentSubscriptionByTenantIdAsync(Guid tenantId);
    Task<TenantSubscription?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<TenantSubscription>> GetExpiringTrialsAsync(DateTime beforeDate);
    Task<IEnumerable<TenantSubscription>> GetDueBillingsAsync(DateTime onOrBeforeDate);
    Task<IEnumerable<TenantSubscription>> GetByStatusAsync(SubscriptionStatus status);
    Task<bool> HasActiveSubscriptionAsync(Guid tenantId);
}
