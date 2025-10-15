using RendevumVar.Core.Enums;

namespace RendevumVar.Application.DTOs;

// Staff invitation request
public class InviteStaffDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public Guid SalonId { get; set; }
    public Guid? RoleId { get; set; }
    public string? Specialization { get; set; }
}

// Accept invitation request
public class AcceptInvitationDto
{
    public required string Token { get; set; }
    public required string Password { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
}

// Update staff profile
public class UpdateStaffProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public string? PhotoUrl { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? CommissionRate { get; set; }
}

// Staff response DTO
public class StaffDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SalonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public decimal AverageRating { get; set; }
    public StaffStatus Status { get; set; }
    public InvitationStatus InvitationStatus { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? CommissionRate { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public RoleDto? Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Role DTO
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
    public bool IsSystemRole { get; set; }
}

// Create role request
public class CreateRoleDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

// Staff schedule DTO
public class StaffScheduleDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public WorkDayOfWeek? DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime? SpecificDate { get; set; }
    public bool IsActive { get; set; }
}

// Create/Update schedule request
public class SetStaffScheduleDto
{
    public WorkDayOfWeek? DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakStartTime { get; set; }
    public TimeSpan? BreakEndTime { get; set; }
    public bool IsRecurring { get; set; } = true;
    public DateTime? SpecificDate { get; set; }
}

// Time off request DTO
public class TimeOffRequestDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public TimeOffType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysRequested { get; set; }
    public string? Reason { get; set; }
    public TimeOffStatus Status { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Create time off request
public class CreateTimeOffRequestDto
{
    public TimeOffType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

// Approve/Reject time off request
public class ProcessTimeOffRequestDto
{
    public TimeOffStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}

// Available time slot
public class AvailableTimeSlotDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationMinutes { get; set; }
}
