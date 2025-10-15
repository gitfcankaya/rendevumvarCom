using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Repositories;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public class SubscriptionPlanRepository : Repository<SubscriptionPlan>, ISubscriptionPlanRepository
{
    public SubscriptionPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SubscriptionPlan>> GetActiveActivePlansAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetByIdWithSubscriptionsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.TenantSubscriptions)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<bool> HasActiveSubscribersAsync(Guid planId)
    {
        return await _context.TenantSubscriptions
            .AnyAsync(ts => ts.SubscriptionPlanId == planId 
                           && (ts.Status == Core.Enums.SubscriptionStatus.Active 
                               || ts.Status == Core.Enums.SubscriptionStatus.Trialing)
                           && !ts.IsDeleted);
    }
}
