namespace RendevumVar.Core.Entities;

public class Staff : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public Guid UserId { get; set; }
    public string? Specialties { get; set; } // JSON array
    public string? Bio { get; set; }
    public string? WorkingHours { get; set; } // JSON
    public decimal AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Salon Salon { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
}
