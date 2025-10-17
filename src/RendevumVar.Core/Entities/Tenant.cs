namespace RendevumVar.Core.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = "Free";
    public string Status { get; set; } = "Active";
    public Guid OwnerId { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Salon> Salons { get; set; } = new List<Salon>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
