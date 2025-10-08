namespace RendevumVar.Core.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = "Free";
    public string Status { get; set; } = "Active";

    // Navigation properties
    public ICollection<Salon> Salons { get; set; } = new List<Salon>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
