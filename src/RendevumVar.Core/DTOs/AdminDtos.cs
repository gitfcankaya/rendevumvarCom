using RendevumVar.Core.Enums;

namespace RendevumVar.Core.DTOs;

// Admin - User Management DTOs
public class AdminUserListDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminUserDetailDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalReviews { get; set; }
    public decimal TotalSpent { get; set; }
}

public class UpdateUserRoleDto
{
    public UserRole Role { get; set; }
}

public class UserFilterDto
{
    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// Admin - Salon Approval DTOs
public class PendingSalonDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public SalonStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class ApproveSalonDto
{
    public string? ApprovalNotes { get; set; }
}

public class RejectSalonDto
{
    public string RejectionReason { get; set; } = string.Empty;
}

// Admin - System Settings DTOs
public class SystemSettingsDto
{
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool AllowNewRegistrations { get; set; }
    public bool AllowNewSalons { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }
    public int MaxAppointmentsPerDay { get; set; }
    public int AppointmentCancellationHours { get; set; }
    public Dictionary<string, bool> FeatureFlags { get; set; } = new();
}

public class UpdateSystemSettingsDto
{
    public bool? MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool? AllowNewRegistrations { get; set; }
    public bool? AllowNewSalons { get; set; }
    public bool? EmailNotificationsEnabled { get; set; }
    public bool? SmsNotificationsEnabled { get; set; }
    public int? MaxAppointmentsPerDay { get; set; }
    public int? AppointmentCancellationHours { get; set; }
}

public class FeatureFlagDto
{
    public string Key { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
}
