using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class Staff : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public Guid? UserId { get; set; } // Nullable until invitation is accepted
    
    // Personal Information
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    
    // Role & Permissions
    public Guid? RoleId { get; set; }
    public Role? Role { get; set; }
    
    // Status Management
    public StaffStatus Status { get; set; } = StaffStatus.Invited;
    
    // Invitation Management
    public InvitationStatus InvitationStatus { get; set; } = InvitationStatus.Pending;
    public string? InvitationToken { get; set; }
    public DateTime? InvitationSentAt { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }
    public DateTime? InvitationAcceptedAt { get; set; }
    
    // Employment Details
    public DateTime? HireDate { get; set; }
    public string? Specialties { get; set; } // JSON array
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Legacy Fields (keeping for backward compatibility)
    public string? WorkingHours { get; set; } // JSON - deprecated, use StaffSchedule instead
    public decimal AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    
    // Compensation (for future payroll features)
    public decimal? HourlyRate { get; set; }
    public decimal? CommissionRate { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Salon Salon { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
    public ICollection<StaffSchedule> Schedules { get; set; } = new List<StaffSchedule>();
    public ICollection<TimeOffRequest> TimeOffRequests { get; set; } = new List<TimeOffRequest>();
    
    // Full Name Helper
    public string FullName => $"{FirstName} {LastName}";
}
