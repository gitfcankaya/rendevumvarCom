namespace RendevumVar.Core.Entities;

public class Review : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid SalonId { get; set; }
    public Guid? StaffId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public string? Response { get; set; }
    public Guid? ResponseBy { get; set; }
    public DateTime? ResponseAt { get; set; }
    public bool IsPublished { get; set; } = true;

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Salon Salon { get; set; } = null!;
    public Staff? Staff { get; set; }
    public User? ResponseByUser { get; set; }
}
