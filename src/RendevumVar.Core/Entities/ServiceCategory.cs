namespace RendevumVar.Core.Entities;

public class ServiceCategory : BaseEntity
{
    public Guid? TenantId { get; set; } // Null for global categories
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
