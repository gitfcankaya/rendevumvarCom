using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class Salon : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? BusinessHours { get; set; } // JSON
    public decimal AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Approval fields
    public SalonStatus Status { get; set; } = SalonStatus.Pending;
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<SalonImage> Images { get; set; } = new List<SalonImage>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Staff> Staff { get; set; } = new List<Staff>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
