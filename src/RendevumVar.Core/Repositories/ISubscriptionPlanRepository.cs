using RendevumVar.Core.Entities;

namespace RendevumVar.Core.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IEnumerable<SubscriptionPlan>> GetActiveActivePlansAsync();
    Task<SubscriptionPlan?> GetByIdWithSubscriptionsAsync(Guid id);
    Task<bool> HasActiveSubscribersAsync(Guid planId);
}
