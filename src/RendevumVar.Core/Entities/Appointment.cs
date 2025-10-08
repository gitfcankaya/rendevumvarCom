using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class Appointment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StaffId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CustomerNotes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DepositPaid { get; set; } = 0;
    public bool ReminderSent { get; set; } = false;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Salon Salon { get; set; } = null!;
    public User Customer { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Review? Review { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
