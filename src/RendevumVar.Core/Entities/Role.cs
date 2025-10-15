namespace RendevumVar.Core.Entities;

public class Role : BaseEntity
{
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false; // System roles cannot be deleted
    
    // Permissions (stored as JSON array of permission strings)
    public string Permissions { get; set; } = "[]";
    
    // Navigation Properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}
