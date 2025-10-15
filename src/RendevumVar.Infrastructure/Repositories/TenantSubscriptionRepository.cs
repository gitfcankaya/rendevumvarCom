using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class TenantSubscriptionRepository : Repository<TenantSubscription>, ITenantSubscriptionRepository
{
    public TenantSubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TenantSubscription?> GetCurrentSubscriptionByTenantIdAsync(Guid tenantId)
    {
        return await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.Invoices)
            .Where(ts => ts.TenantId == tenantId && !ts.IsDeleted)
            .OrderByDescending(ts => ts.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<TenantSubscription?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.Tenant)
            .Include(ts => ts.Invoices)
            .FirstOrDefaultAsync(ts => ts.Id == id && !ts.IsDeleted);
    }

    public async Task<IEnumerable<TenantSubscription>> GetExpiringTrialsAsync(DateTime beforeDate)
    {
        return await _dbSet
            .Include(ts => ts.Tenant)
            .Include(ts => ts.SubscriptionPlan)
            .Where(ts => ts.Status == SubscriptionStatus.Trialing
                        && ts.TrialEndDate.HasValue
                        && ts.TrialEndDate.Value <= beforeDate
                        && !ts.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TenantSubscription>> GetDueBillingsAsync(DateTime onOrBeforeDate)
    {
        return await _dbSet
            .Include(ts => ts.Tenant)
            .Include(ts => ts.SubscriptionPlan)
            .Where(ts => ts.Status == SubscriptionStatus.Active
                        && ts.NextBillingDate.HasValue
                        && ts.NextBillingDate.Value <= onOrBeforeDate
                        && ts.AutoRenew
                        && !ts.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TenantSubscription>> GetByStatusAsync(SubscriptionStatus status)
    {
        return await _dbSet
            .Include(ts => ts.Tenant)
            .Include(ts => ts.SubscriptionPlan)
            .Where(ts => ts.Status == status && !ts.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid tenantId)
    {
        return await _dbSet
            .AnyAsync(ts => ts.TenantId == tenantId
                           && (ts.Status == SubscriptionStatus.Active || ts.Status == SubscriptionStatus.Trialing)
                           && !ts.IsDeleted);
    }
}
