using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

public class TimeOffRequest : BaseEntity
{
    public Guid StaffId { get; set; }
    public Guid TenantId { get; set; }
    
    // Request Details
    public TimeOffType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    
    // Approval
    public TimeOffStatus Status { get; set; } = TimeOffStatus.Pending;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // Navigation Properties
    public Staff Staff { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    
    // Helper Properties
    public int DaysRequested => (EndDate.Date - StartDate.Date).Days + 1;
}
