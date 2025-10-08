namespace RendevumVar.Core.Entities;

public class Service : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Salon Salon { get; set; } = null!;
    public ServiceCategory? Category { get; set; }
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
