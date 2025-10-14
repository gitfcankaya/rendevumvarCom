using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class User : BaseEntity
{
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public bool PhoneConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
